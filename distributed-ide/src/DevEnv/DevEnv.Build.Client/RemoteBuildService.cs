using DevEnv.Base.RuntimeChecks;
using DevEnv.Base.Settings;
using Grpc.Net.Client;
using GrpcBuild;

namespace DevEnv.Build.Client
{
    /// <summary>
    /// Implements <see cref="IBuildService"/>, and encapsulates the generated client class.
    /// </summary>
    public class RemoteBuildService : IBuildService, IDisposable
    {
        private readonly GrpcChannel channel;
        private readonly BuildService.BuildServiceClient client;

        public RemoteBuildService(ISettingsProvider<ISettings> settingsProvider)
        {
            Argument.AssertNotNull(settingsProvider, nameof(settingsProvider));

            var settings = settingsProvider.GetSettings();
            this.channel = GrpcChannel.ForAddress(settings.BuildServiceAddress, new GrpcChannelOptions
            {
                MaxReceiveMessageSize = 100 * 1024 * 1024,
                MaxSendMessageSize = 100 * 1024 * 1024,
            });

            this.client = new BuildService.BuildServiceClient(this.channel);
        }

        public void Dispose()
        {
            this.channel.Dispose();
        }

        public Task<BuildResult> BuildDotnet(string workDirId, string solutionFile)
        {
            Argument.AssertNotEmpty(workDirId, nameof(workDirId));
            Argument.AssertNotEmpty(solutionFile, nameof(solutionFile));

            return this.Build(workDirId, BuildSystem.Dotnet, solutionFile);
        }

        public async Task<BuildResult> Build(string workDirId, BuildSystem buildSystem, string solutionFile)
        {
            Argument.AssertNotEmpty(workDirId, nameof(workDirId));
            Argument.AssertNotEmpty(solutionFile, nameof(solutionFile));

            var reply = await this.client.BuildAsync(new BuildRequest
            {
                WorkDirId = workDirId,
                RootFilePath = solutionFile,
                BuildSystem = buildSystem,
            });

            return new BuildResult(reply.BuildId, reply.BuildStatus == BuildStatus.Successful, reply.BuildOutput);
        }

        public async Task<IEnumerable<FileRevision>> GetArtifactFiles(string workDirId, string buildId)
        {
            Argument.AssertNotEmpty(workDirId, nameof(workDirId));
            Argument.AssertNotEmpty(buildId, nameof(buildId));

            var reply = await this.client.GetArtifactFilesAsync(new GetArtifactFilesRequest
            {
                WorkDirId = workDirId,
                BuildId = buildId,
            });

            var files = reply.Files
                .Select(f => new FileRevision(f.FilePath, f.RevisionNumber))
                .ToList();

            return files;
        }

        public async Task<byte[]> LoadArtifactFile(string workDirId, string buildId, string filePath)
        {
            Argument.AssertNotEmpty(workDirId, nameof(workDirId));
            Argument.AssertNotEmpty(buildId, nameof(buildId));
            Argument.AssertNotEmpty(filePath, nameof(filePath));

            var reply = await this.client.LoadArtifactFileAsync(new LoadArtifactFileRequest
            {
                WorkDirId = workDirId,
                BuildId = buildId,
                FilePath = filePath,
            });

            return reply.Content.ToByteArray();
        }
    }
}