using DevEnv.Base.Caching;
using DevEnv.Base.FileSystem;
using DevEnv.Base.RuntimeChecks;
using DevEnv.Build.Service.BuildArtifacts;
using DevEnv.Build.Service.BuildArtifacts.MetaData;
using DevEnv.Build.Service.BuildSystems;
using DevEnv.WorkDir.Client;
using Google.Protobuf;
using Grpc.Core;
using GrpcBuild;

namespace DevEnv.Build.Service.Services
{
    /// <summary>
    /// The Build Service API.
    /// </summary>
    public class BuildService : GrpcBuild.BuildService.BuildServiceBase
    {
        private readonly ILogger<BuildService> logger;
        private readonly IBuildArtifactsHelper buildArtifactsHelper;
        private readonly IWorkDirService workDirService;
        private readonly IFileCacheManager fileCacheManager;
        private readonly IFileSystemUtils fileSystemUtils;
        private readonly IBuildSystemProvider buildStrategyProvider;

        public BuildService(
            ILogger<BuildService> logger,
            IBuildArtifactsHelper buildArtifactsHelper,
            IWorkDirService workDirService,
            IFileCacheManager fileCacheManager,
            IFileSystemUtils fileSystemUtils,
            IBuildSystemProvider buildStrategyProvider)
        {
            Argument.AssertNotNull(logger, nameof(logger));
            Argument.AssertNotNull(buildArtifactsHelper, nameof(buildArtifactsHelper));
            Argument.AssertNotNull(workDirService, nameof(workDirService));
            Argument.AssertNotNull(fileCacheManager, nameof(fileCacheManager));
            Argument.AssertNotNull(fileSystemUtils, nameof(fileSystemUtils));
            Argument.AssertNotNull(buildStrategyProvider, nameof(buildStrategyProvider));

            this.logger = logger;
            this.buildArtifactsHelper = buildArtifactsHelper;
            this.workDirService = workDirService;
            this.fileCacheManager = fileCacheManager;
            this.fileSystemUtils = fileSystemUtils;
            this.buildStrategyProvider = buildStrategyProvider;
        }

        /// <summary>
        /// Builds code from a specified work dir, based on a specified build definition root file, and using a specified build system.
        /// </summary>
        public async override Task<BuildReply> Build(BuildRequest request, ServerCallContext context)
        {
            Argument.AssertNotNull(request, nameof(request));
            Argument.AssertNotNull(context, nameof(context));

            var buildSystem = this.buildStrategyProvider.GetSpecificBuildStrategy(request.BuildSystem);

            // Download files to cache.
            var fileRevisions = await this.workDirService.GetFiles(request.WorkDirId);
            var missingFileRevisions = fileRevisions
                .Where(f => !this.fileCacheManager.IsChached(request.WorkDirId, f.FileName, f.Revision))
                .ToList();

            var cacheFolder = this.buildArtifactsHelper.GetCacheDirectoryPath(request.WorkDirId);
            this.fileSystemUtils.CreateDirectoryIfNecessary(cacheFolder);

            foreach (var fileRevision in missingFileRevisions)
            {
                this.logger.LogInformation($"Downloading {fileRevision.FileName}");

                var fileContent = await this.workDirService.LoadFile(request.WorkDirId, fileRevision.FileName);
                var filePath = Path.Combine(cacheFolder, fileRevision.FileName);
                this.fileSystemUtils.CreateDirectoryIfNecessary(filePath);
                File.WriteAllBytes(filePath, fileContent);

                this.fileCacheManager.RegisterCachedFile(request.WorkDirId, fileRevision.FileName, fileRevision.Revision);
            }

            // Stage for build.
            using var tempBuildDir = new TempDirectory(this.buildArtifactsHelper.GetBuildBaseDirectoryPath(request.WorkDirId));
            this.logger.LogInformation($"Temp build dir \"{tempBuildDir.FullName}\" created.");

            foreach (var fileRevision in fileRevisions)
            {
                var cacheFilePath = Path.Combine(cacheFolder, fileRevision.FileName);
                var buildFilePath = Path.Combine(tempBuildDir.FullName, fileRevision.FileName);

                // Create directories if necessary.
                this.fileSystemUtils.CreateDirectoryIfNecessary(buildFilePath);

                // Copy the file.
                File.Copy(cacheFilePath, buildFilePath);
            }

            // Build (build-system-specific).
            var a = tempBuildDir.FullName;
            var b = request.RootFilePath.TrimStart('\\');
            var buildRootFilePath = Path.Combine(a, b);

            if (!File.Exists(buildRootFilePath))
            {
                throw new InvalidOperationException($"The build root file \"{buildRootFilePath}\" was not found.");
            }

            var result = await buildSystem.Build(tempBuildDir.FullName, buildRootFilePath);

            // Prepare ID + artifacts directory.
            var buildId = Guid.NewGuid().ToString("B");
            var buildArtifactsDirectoryPath = this.buildArtifactsHelper.GetBuildArtifactsDirectoryPath(request.WorkDirId, buildId);
            Directory.CreateDirectory(buildArtifactsDirectoryPath);

            // Copy artifacts (build-system-specific).
            buildSystem.CopyBuildArtifacts(tempBuildDir.FullName, buildArtifactsDirectoryPath);

            // Update all the revision/metadata stuff.
            this.UpdateArtifactsMetadata(request.WorkDirId, buildId);

            return new BuildReply
            {
                BuildId = buildId,
                BuildStatus = result.Successful ? BuildStatus.Successful : BuildStatus.Failed,
                BuildOutput = result.Output,
            };
        }

        /// <summary>
        /// Gets a list of all the artifact files, incl. their revision number, from a specific build.
        /// </summary>
        public override Task<GetArtifactFilesReply> GetArtifactFiles(GetArtifactFilesRequest request, ServerCallContext context)
        {
            Argument.AssertNotNull(request, nameof(request));
            Argument.AssertNotNull(context, nameof(context));

            var buildArtifactsDir = this.buildArtifactsHelper.GetBuildArtifactsDirectoryPath(request.WorkDirId, request.BuildId);
            var buildArtifactsMetadata = this.buildArtifactsHelper.GetBuildSpecificArtifactsMetadata(request.WorkDirId, request.BuildId);

            var fileRevisions = buildArtifactsMetadata.Artifacts
                .Select(a => new GrpcBuild.FileRevision
                {
                    FilePath = a.FilePath,
                    RevisionNumber = a.CurrentRevisionNumber!.Value,
                })
                .ToList();

            var reply = new GetArtifactFilesReply();
            reply.Files.Add(fileRevisions);

            return Task.FromResult(reply);
        }

        /// <summary>
        /// Loads the content of an artifacts file from a specific build.
        /// </summary>
        public override Task<LoadArtifactFileReply> LoadArtifactFile(LoadArtifactFileRequest request, ServerCallContext context)
        {
            Argument.AssertNotNull(request, nameof(request));
            Argument.AssertNotNull(context, nameof(context));

            var buildArtifactsDir = this.buildArtifactsHelper.GetBuildArtifactsDirectoryPath(request.WorkDirId, request.BuildId);
            var artifactFilePath = Path.Combine(buildArtifactsDir, request.FilePath);

            using var fileStream = File.OpenRead(artifactFilePath);
            var fileContent = ByteString.FromStream(fileStream);
            var reply = new LoadArtifactFileReply
            {
                FilePath = request.FilePath,
                Content = fileContent,
            };

            return Task.FromResult(reply);
        }

        private void UpdateArtifactsMetadata(string workDirId, string buildId)
        {
            Argument.AssertNotEmpty(workDirId, nameof(workDirId));
            Argument.AssertNotEmpty(buildId, nameof(buildId));

            var totalMetadata = this.buildArtifactsHelper.GetTotalArtifactsMetadata(workDirId);

            var newBuildArtifactsDir = this.buildArtifactsHelper.GetBuildArtifactsDirectoryPath(workDirId, buildId);
            var newBuildFiles = Directory.GetFiles(newBuildArtifactsDir, "*", SearchOption.AllDirectories);

            var newBuildArtifactInfos = new List<BuildArtifactInfo>();

            foreach (var newFile in newBuildFiles)
            {
                var newFileRevision = this.DetermineAndUpdateFileRevision(
                    workDirId,
                    buildId,
                    newBuildArtifactsDir,
                    newFile,
                    totalMetadata);

                // Also add a revision info to the build-specific metadata.
                newBuildArtifactInfos.Add(new BuildArtifactInfo
                    {
                        FilePath = Path.GetRelativePath(newBuildArtifactsDir, newFile),
                        BuildId = buildId,
                        CurrentRevisionNumber = newFileRevision,
                    });
            }

            // Persist the total and build-specific metadata.
            this.buildArtifactsHelper.UpdatePersistBuildSpecificArtifactsMetadata(workDirId, buildId, m =>
                {
                    m.Artifacts = newBuildArtifactInfos;
                });

            this.buildArtifactsHelper.UpdatePersistTotalArtifactsMetadata(workDirId, m =>
                {
                    m.Artifacts = totalMetadata.Artifacts;
                });
        }

        private int DetermineAndUpdateFileRevision(
            string workDirId,
            string buildId,
            string newBuildArtifactsDir,
            string newBuildFilePath,
            BuildArtifactsMetadata totalMetadata)
        {
            Argument.AssertNotEmpty(workDirId, nameof(workDirId));
            Argument.AssertNotEmpty(buildId, nameof(buildId));
            Argument.AssertNotEmpty(newBuildArtifactsDir, nameof(newBuildArtifactsDir));
            Argument.AssertNotEmpty(newBuildFilePath, nameof(newBuildFilePath));
            Argument.AssertNotNull(totalMetadata, nameof(totalMetadata));

            var buildFilePathRelative = Path.GetRelativePath(newBuildArtifactsDir, newBuildFilePath);
            var existingArtifactInfo = totalMetadata.Artifacts.SingleOrDefault(a => a.FilePath == buildFilePathRelative);

            if (existingArtifactInfo == null)
            {
                // The artifact is all new.
                totalMetadata.Artifacts.Add(new BuildArtifactInfo
                {
                    FilePath = buildFilePathRelative,
                    BuildId = buildId,
                    CurrentRevisionNumber = 1,
                });

                return 1;
            }

            // The artifact exists in an older build.
            var existingFileDir = this.buildArtifactsHelper.GetBuildArtifactsDirectoryPath(workDirId, existingArtifactInfo.BuildId!);
            var existingFilePath = Path.Combine(existingFileDir, existingArtifactInfo.FilePath!);

            if (!this.fileSystemUtils.AreFilesEqual(newBuildFilePath, existingFilePath))
            {
                // The files are different. The revision number is incremented.
                // The total metadata is updated directly... a little ugly.
                existingArtifactInfo.BuildId = buildId;
                existingArtifactInfo.CurrentRevisionNumber++;
            }

            return existingArtifactInfo.CurrentRevisionNumber!.Value;
        }
    }
}