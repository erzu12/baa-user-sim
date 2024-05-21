using DevEnv.Base.Caching;
using DevEnv.Base.FileSystem;
using DevEnv.Base.Processes;
using DevEnv.Base.RuntimeChecks;
using DevEnv.Base.Settings;
using DevEnv.Build.Client;
using DevEnv.Execute.Service.Execution;
using Grpc.Core;
using GrpcExecute;

namespace DevEnv.Execute.Service.Services
{
    /// <summary>
    /// The Execute Service API.
    /// </summary>
    public class ExecuteService : GrpcExecute.ExecuteService.ExecuteServiceBase
    {
        private readonly ILogger<ExecuteService> logger;
        private readonly ISettings settings;
        private readonly IBuildService buildService;
        private readonly IFileCacheManager fileCacheManager;
        private readonly IFileSystemUtils fileSystemUtils;
        private readonly IExecutorProvider executorProvider;
        private readonly IProcessManager processManager;

        public ExecuteService(
            ILogger<ExecuteService> logger,
            ISettingsProvider<ISettings> settingsProvider,
            IBuildService buildService,
            IFileCacheManager fileCacheManager,
            IFileSystemUtils fileSystemUtils,
            IExecutorProvider executorProvider,
            IProcessManager processManager)
        {
            Argument.AssertNotNull(logger, nameof(logger));
            Argument.AssertNotNull(settingsProvider, nameof(settingsProvider));
            Argument.AssertNotNull(buildService, nameof(buildService));
            Argument.AssertNotNull(fileCacheManager, nameof(fileCacheManager));
            Argument.AssertNotNull(fileSystemUtils, nameof(fileSystemUtils));
            Argument.AssertNotNull(executorProvider, nameof(executorProvider));
            Argument.AssertNotNull(processManager, nameof(processManager));

            this.logger = logger;
            this.settings = settingsProvider.GetSettings();
            this.buildService = buildService;
            this.fileCacheManager = fileCacheManager;
            this.fileSystemUtils = fileSystemUtils;
            this.executorProvider = executorProvider;
            this.processManager = processManager;
        }

        public async override Task<StartExecuteReply> StartExecute(StartExecuteRequest request, ServerCallContext context)
        {
            Argument.AssertNotNull(request, nameof(request));
            Argument.AssertNotNull(context, nameof(context));

            var executor = this.executorProvider.GetExecutor(request.Environment);

            var workDirPath = this.GetWorkingDirectoryPath(request.WorkDirId);
            this.fileSystemUtils.CreateDirectoryIfNecessary(workDirPath);
            
            // Download necessary artifact files.
            var artifactFiles = await this.buildService.GetArtifactFiles(request.WorkDirId, request.BuildId);
            var missingArtifactFiles = artifactFiles
                .Where(f => !this.fileCacheManager.IsChached(request.WorkDirId, f.FileName, f.Revision))
                .ToList();
            
            foreach (var file in missingArtifactFiles)
            {
                this.logger.LogInformation($"Downloading artifact \"{file.FileName}\"...");
                var fileBytes = await this.buildService.LoadArtifactFile(request.WorkDirId, request.BuildId, file.FileName);
                var newFilePath = Path.Combine(workDirPath, file.FileName);

                this.fileSystemUtils.CreateDirectoryIfNecessary(newFilePath);
                File.WriteAllBytes(newFilePath, fileBytes);

                // Update cache.
                this.fileCacheManager.RegisterCachedFile(request.WorkDirId, file.FileName, file.Revision);
            }

            var executeFilePath = Path.Combine(workDirPath, request.RootFilePath);
            var executeWorkingDirectory = Path.GetDirectoryName(executeFilePath) ?? string.Empty;

            var executeId = executor.StartExecute(executeWorkingDirectory, executeFilePath, this.processManager);

            return new StartExecuteReply
            {
                ExecuteId = executeId,
            };
        }

        public override Task<GetStatusReply> GetStatus(GetStatusRequest request, ServerCallContext context)
        {
            Argument.AssertNotNull(request, nameof(request));
            Argument.AssertNotNull(context, nameof(context));

            var status = this.processManager.GetStatus(request.ExecuteId);

            return Task.FromResult(new GetStatusReply
            {
                HasExited = status.HasExited,
                ExitCode = status.ExitCode ?? 0,
            });
        }

        public override Task<GetOutputLinesReply> GetOutputLines(GetOutputLinesRequest request, ServerCallContext context)
        {
            Argument.AssertNotNull(request, nameof(request));
            Argument.AssertNotNull(context, nameof(context));

            var outputLines = this.processManager.GetOutputLines(request.ExecuteId, request.Offset);
            var reply = new GetOutputLinesReply();
            reply.OutputLines.AddRange(outputLines);

            return Task.FromResult(reply);
        }

        public override Task<EmptyReply> Kill(KillRequest request, ServerCallContext context)
        {
            Argument.AssertNotNull(request, nameof(request));
            Argument.AssertNotNull(context, nameof(context));

            this.processManager.Kill(request.ExecuteId);

            return Task.FromResult(new EmptyReply());
        }

        private string GetWorkingDirectoryPath(string workDirId)
        {
            Argument.AssertNotEmpty(workDirId, nameof(workDirId));

            return Path.Combine(this.settings.DataDirectoryPath, workDirId);
        }
    }
}