using DevEnv.Base.RuntimeChecks;
using DevEnv.Base.Settings;
using Grpc.Net.Client;
using GrpcExecute;

namespace DevEnv.Execute.Client
{
    /// <summary>
    /// Implements <see cref="IExecuteService"/>, and encapsulates the generated client class.
    /// </summary>
    public class RemoteExecuteService : IExecuteService, IDisposable
    {
        private readonly GrpcChannel channel;
        private readonly ExecuteService.ExecuteServiceClient client;

        public RemoteExecuteService(ISettingsProvider<ISettings> settingsProvider)
        {
            Argument.AssertNotNull(settingsProvider, nameof(settingsProvider));

            var settings = settingsProvider.GetSettings();
            this.channel = GrpcChannel.ForAddress(settings.ExecuteServiceAddress, new GrpcChannelOptions
            {
                MaxReceiveMessageSize = 100 * 1024 * 1024,
                MaxSendMessageSize = 100 * 1024 * 1024,
            });

            this.client = new ExecuteService.ExecuteServiceClient(this.channel);
        }

        public async Task<string> StartExecuteDotnet(string workDirId, string buildId, string executeFile)
        {
            Argument.AssertNotEmpty(workDirId, nameof(workDirId));
            Argument.AssertNotEmpty(buildId, nameof(buildId));
            Argument.AssertNotEmpty(executeFile, nameof(executeFile));

            var reply = await this.client.StartExecuteAsync(new StartExecuteRequest
                {
                    WorkDirId = workDirId,
                    BuildId = buildId,
                    Environment = GrpcExecute.Environment.DotnetExe,
                    RootFilePath = executeFile,
                });

            return reply.ExecuteId;
        }

        public async Task<string> StartExecute(string workDirId, string buildId, GrpcExecute.Environment environment, string executeFile)
        {
            Argument.AssertNotEmpty(workDirId, nameof(workDirId));
            Argument.AssertNotEmpty(buildId, nameof(buildId));
            Argument.AssertNotEmpty(executeFile, nameof(executeFile));

            var reply = await this.client.StartExecuteAsync(new StartExecuteRequest
            {
                WorkDirId = workDirId,
                BuildId = buildId,
                Environment = environment,
                RootFilePath = executeFile,
            });

            return reply.ExecuteId;
        }

        public async Task<StatusResult> GetStatus(string executeId)
        {
            Argument.AssertNotEmpty(executeId, nameof(executeId));

            var reply = await this.client.GetStatusAsync(new GetStatusRequest { ExecuteId = executeId });

            return new StatusResult(reply.HasExited, reply.ExitCode);
        }

        public async Task<IReadOnlyList<string>> GetOutputLines(string executeId, int offset)
        {
            Argument.AssertNotEmpty(executeId, nameof(executeId));

            var reply = await this.client.GetOutputLinesAsync(new GetOutputLinesRequest
                { 
                    ExecuteId = executeId,
                    Offset = offset
                });

            var lines = reply.OutputLines.ToList();

            return lines;
        }

        public async Task Kill(string executeId)
        {
            Argument.AssertNotEmpty(executeId, nameof(executeId));

            await this.client.KillAsync(new KillRequest { ExecuteId = executeId });
        }

        public void Dispose()
        {
            this.channel.Dispose();
        }
    }
}
