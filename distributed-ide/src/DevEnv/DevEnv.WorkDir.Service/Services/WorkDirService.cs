using DevEnv.Base.RuntimeChecks;
using DevEnv.Base.Settings;
using DevEnv.WorkDir.Service.Git;
using DevEnv.WorkDir.Service.WorkingDirectory;
using Google.Protobuf;
using Grpc.Core;
using GrpcWorkDir;

namespace DevEnv.WorkDir.Service.Services
{
    /// <summary>
    /// The WorkDir Service API.
    /// </summary>
    public class WorkDirService : GrpcWorkDir.WorkDirService.WorkDirServiceBase
    {
        private readonly ILogger<WorkDirService> logger;
        private readonly ISettings settings;
        private readonly IWorkDirHelper workDirHelper;
        private readonly IGitCommandHelper gitCommandHelper;

        public WorkDirService(
            ILogger<WorkDirService> logger,
            ISettingsProvider<ISettings> settingsProvider,
            IWorkDirHelper workDirHelper,
            IGitCommandHelper gitCommandHelper)
        {
            Argument.AssertNotNull(logger, nameof(logger));
            Argument.AssertNotNull(settingsProvider, nameof(settingsProvider));
            Argument.AssertNotNull(workDirHelper, nameof(workDirHelper));
            Argument.AssertNotNull(gitCommandHelper, nameof(gitCommandHelper));

            this.logger = logger;
            this.settings = settingsProvider.GetSettings();
            this.workDirHelper = workDirHelper;
            this.gitCommandHelper = gitCommandHelper;
        }

        public async override Task<StartNewWorkDirReply> StartNewWorkDir(StartNewWorkDirRequest request, ServerCallContext context)
        {
            Argument.AssertNotNull(request, nameof(request));
            Argument.AssertNotNull(context, nameof(context));

            // Generate an ID.
            var id = Guid.NewGuid().ToString("B");

            // Create a directory.
            var newWorkingDirPath = Path.Combine(this.settings.DataDirectoryPath, id);
            _ = Directory.CreateDirectory(newWorkingDirPath);

            // Clone the repo.
            var cloneResult = await this.gitCommandHelper.GitClone(newWorkingDirPath, request.RepoUrl);
            this.logger.Log(LogLevel.Information, cloneResult.Output);

            var repoDirectoryName = this.workDirHelper.GetRepoDirName(newWorkingDirPath);

            // Store metadata.
            this.workDirHelper.UpdatePersistMetadata(id, m =>
            {
                m.Id = id;
                m.Description = request.Description;
                m.RepoUrl = request.RepoUrl;
                m.Branch = request.Branch;
                m.RepoDirName = repoDirectoryName;
            });

            // Checkout the selected branch.
            var checkoutResult = await this.gitCommandHelper.ExecuteGitCommand(Path.Combine(newWorkingDirPath, repoDirectoryName), $"checkout -b {request.Branch}");

            this.logger.Log(LogLevel.Information, checkoutResult.Output);

            return new StartNewWorkDirReply
            {
                Id = id,
            };
        }

        public override Task<GetFilesReply> GetFiles(GetFilesRequest request, ServerCallContext context)
        {
            Argument.AssertNotNull(request, nameof(request));
            Argument.AssertNotNull(context, nameof(context));

            var metadata = this.workDirHelper.GetMetadata(request.WorkDirId);

            var workDirPath = this.workDirHelper.PrepareAndVerifyWorkDirPath(request.WorkDirId);
            var repoDirPath = this.workDirHelper.GetRepoDirPath(workDirPath);

            var fileNames = Directory.GetFiles(repoDirPath, "*", new EnumerationOptions { RecurseSubdirectories = true });
            var filePathsRelative = fileNames
                .Select(f => Path.GetRelativePath(repoDirPath, f))
                .ToList();

            var fileRevisions = filePathsRelative
                .Select(p => new FileRevision
                {
                    FilePath = p,
                    RevisionNumber = this.workDirHelper.DetermineCurrentRevisionNumber(p, metadata.FileRevisions),
                })
                .ToList();

            var reply = new GetFilesReply();
            reply.Files.AddRange(fileRevisions);

            return Task.FromResult(reply);
        }

        public override Task<LoadFileReply> LoadFile(LoadFileRequest request, ServerCallContext context)
        {
            Argument.AssertNotNull(request, nameof(request));
            Argument.AssertNotNull(context, nameof(context));

            var filePath = this.workDirHelper.PrepareAndVerifyFilePath(request.WorkDirId, request.FilePath);

            using var fileStream = File.OpenRead(filePath);
            var fileContent = ByteString.FromStream(fileStream);
            var reply = new LoadFileReply
            {
                FilePath = request.FilePath,
                Content = fileContent,
            };

            return Task.FromResult(reply);
        }

        public override Task<EmptyReply> UpdateFile(UpdateFileRequest request, ServerCallContext context)
        {
            Argument.AssertNotNull(request, nameof(request));
            Argument.AssertNotNull(context, nameof(context));

            var filePathAbsolute = this.workDirHelper.PrepareAndVerifyFilePath(request.WorkDirId, request.FilePath);

            this.workDirHelper.UpdatePersistMetadata(request.WorkDirId, m =>
                this.workDirHelper.IncrementRevisionNumber(request.FilePath, m.FileRevisions));

            File.WriteAllBytes(filePathAbsolute, request.UpdatedContent.ToByteArray());

            var repoDirPathAbsolute = this.workDirHelper.PrepareAndVerifyRepoDirPath(request.WorkDirId);
            _ = this.gitCommandHelper.GitAdd(repoDirPathAbsolute, request.FilePath);

            return Task.FromResult(new EmptyReply());
        }

        public async override Task<EmptyReply> CommitChanges(CommitChangesRequest request, ServerCallContext context)
        {
            Argument.AssertNotNull(request, nameof(request));
            Argument.AssertNotNull(context, nameof(context));

            var repoDirPath = this.workDirHelper.PrepareAndVerifyRepoDirPath(request.WorkDirId);
            var commitResult = await this.gitCommandHelper.GitCommit(repoDirPath, request.CommitMessage);
            this.logger.LogInformation(commitResult.Output);

            var pushResult = await this.gitCommandHelper.GitPush(repoDirPath, request.CommitMessage);
            this.logger.LogInformation(pushResult.Output);

            return new EmptyReply();
        }
    }
}