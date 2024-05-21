using DevEnv.Base.FileSystem;
using DevEnv.Base.Processes;
using DevEnv.Base.RuntimeChecks;

namespace DevEnv.Build.Service.BuildSystems.Dotnet
{
    /// <summary>
    /// Implements <see cref="IBuildSystem"/> for .NET.
    /// </summary>
    public class DotnetBuildSystem : IBuildSystem
    {
        private readonly ILogger<DotnetBuildSystem> logger;
        private readonly IFileSystemUtils fileSystemUtils;

        public DotnetBuildSystem(
            ILogger<DotnetBuildSystem> logger,
            IFileSystemUtils fileSystemUtils)
        {
            Argument.AssertNotNull(logger, nameof(logger));
            Argument.AssertNotNull(fileSystemUtils, nameof(fileSystemUtils));

            this.logger = logger;
            this.fileSystemUtils = fileSystemUtils;
        }

        public async Task<BuildResult> Build(string workingDirectory, string rootFilePath)
        {
            Argument.AssertNotEmpty(workingDirectory, nameof(workingDirectory));
            Argument.AssertNotEmpty(rootFilePath, nameof(rootFilePath));

            var command = $"build \"{rootFilePath}\"";
            this.logger.LogInformation($"dotnet {command}");

            using var processManager = new ProcessManager();
            var executeId = processManager.Start(workingDirectory, "dotnet", command);

            var exitCode = await processManager.Wait(executeId);

            var output = processManager.GetCompleteOutput(executeId);
            this.logger.LogInformation(output);

            return new BuildResult(exitCode == 0, output);
        }

        public void CopyBuildArtifacts(string buildDir, string artifactsDir)
        {
            Argument.AssertNotEmpty(buildDir, nameof(buildDir));
            Argument.AssertNotEmpty(artifactsDir, nameof(artifactsDir));

            var allFolders = Directory.GetDirectories(buildDir, "*", SearchOption.AllDirectories);
            var binFolders = allFolders.Where(f => f.EndsWith("\\bin")).ToList();

            foreach (var binFolder in binFolders)
            {
                var bindaryFiles = Directory.GetFiles(binFolder, "*", SearchOption.AllDirectories);

                foreach (var binaryFilePath in bindaryFiles)
                {
                    var fileName = Path.GetRelativePath(binFolder, binaryFilePath);
                    var targetFilePath = Path.Combine(artifactsDir, fileName);

                    this.fileSystemUtils.CreateDirectoryIfNecessary(targetFilePath);
                    File.Copy(binaryFilePath, targetFilePath, overwrite: true);
                }
            }
        }
    }
}
