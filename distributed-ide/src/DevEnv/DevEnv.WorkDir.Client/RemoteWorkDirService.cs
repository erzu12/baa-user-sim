using DevEnv.Base.RuntimeChecks;
using DevEnv.Base.Settings;
using Google.Protobuf;
using Grpc.Net.Client;
using GrpcWorkDir;

namespace DevEnv.WorkDir.Client
{
    /// <summary>
    /// Implements <see cref="IWorkDirService"/>, and encapsulates the generated client class.
    /// </summary>
    public class RemoteWorkDirService : IWorkDirService, IDisposable
    {
        private readonly GrpcChannel channel;
        private readonly WorkDirService.WorkDirServiceClient client;

        public RemoteWorkDirService(ISettingsProvider<ISettings> settingsProvider)
        {
            Argument.AssertNotNull(settingsProvider, nameof(settingsProvider));

            var settings = settingsProvider.GetSettings();
            this.channel = GrpcChannel.ForAddress(settings.WorkDirServiceAddress, new GrpcChannelOptions
            {
                MaxReceiveMessageSize = 100 * 1024 * 1024,
                MaxSendMessageSize = 100 * 1024 * 1024,
            });

            this.client = new WorkDirService.WorkDirServiceClient(this.channel);
        }

        public void Dispose()
        {
            this.channel.Dispose();
        }

        public async Task<string> StartNewWorkDir(string description, string repoUrl, string branch)
        {
            Argument.AssertNotEmpty(description, nameof(description));
            Argument.AssertNotEmpty(repoUrl, nameof(repoUrl));
            Argument.AssertNotEmpty(branch, nameof(branch));

            var reply = await this.client.StartNewWorkDirAsync(new StartNewWorkDirRequest
            {
                Description = description,
                RepoUrl = repoUrl,
                Branch = branch,
            });

            return reply.Id;
        }

        public async Task<IEnumerable<FileRevision>> GetFiles(string workDirId)
        {
            Argument.AssertNotEmpty(workDirId, nameof(workDirId));

            var reply = await this.client.GetFilesAsync(new GetFilesRequest
            {
                WorkDirId = workDirId,
            });

            var files = reply.Files
                .Select(f => new FileRevision(f.FilePath, f.RevisionNumber))
                .ToList();

            return files;
        }

        public async Task<byte[]> LoadFile(string workDirId, string filePath)
        {
            Argument.AssertNotEmpty(workDirId, nameof(workDirId));
            Argument.AssertNotEmpty(filePath, nameof(filePath));

            var reply = await this.client.LoadFileAsync(new LoadFileRequest
            {
                WorkDirId = workDirId,
                FilePath = filePath,
            });

            return reply.Content.ToByteArray();
        }

        public async Task UpdateFile(string workDirId, string filePath, byte[] updatedFileContent)
        {
            Argument.AssertNotEmpty(workDirId, nameof(workDirId));
            Argument.AssertNotEmpty(filePath, nameof(filePath));
            Argument.AssertNotNull(updatedFileContent, nameof(updatedFileContent));

            _ = await this.client.UpdateFileAsync(new UpdateFileRequest
            {
                WorkDirId = workDirId,
                FilePath = filePath,
                UpdatedContent = ByteString.CopyFrom(updatedFileContent),
            });
        }

        public async Task CommitChanges(string workDirId, string commitMessage)
        {
            Argument.AssertNotEmpty(workDirId, nameof(workDirId));
            Argument.AssertNotEmpty(commitMessage, nameof(commitMessage));

            _ = await this.client.CommitChangesAsync(new CommitChangesRequest
            {
                WorkDirId = workDirId,
                CommitMessage = commitMessage,
            });
        }
    }
}
