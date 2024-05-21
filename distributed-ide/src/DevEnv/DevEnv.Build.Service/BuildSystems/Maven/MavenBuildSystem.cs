using DevEnv.Base.FileSystem;
using DevEnv.Base.Processes;
using DevEnv.Base.RuntimeChecks;

namespace DevEnv.Build.Service.BuildSystems.Maven
{
    /// <summary>
    /// Implements <see cref="IBuildSystem"/> for Maven.
    /// </summary>
    public class MavenBuildSystem : IBuildSystem
    {
        private readonly ILogger<MavenBuildSystem> logger;
        private readonly IFileSystemUtils fileSystemUtils;

        public MavenBuildSystem(
            ILogger<MavenBuildSystem> logger,
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

            var command = $"package -f \"{rootFilePath}\"";
            this.logger.LogInformation($"{command}");

            using var processManager = new ProcessManager();
            var executeId = processManager.Start(workingDirectory, "mvn", command, useShellExecute: true);

            var exitCode = await processManager.Wait(executeId);

            var output = processManager.GetCompleteOutput(executeId);
            this.logger.LogInformation(output);

            return new BuildResult(exitCode == 0, output);
        }

        public void CopyBuildArtifacts(string buildDir, string artifactsDir)
        {
            var allFolders = Directory.GetDirectories(buildDir, "*", SearchOption.AllDirectories);
            var targetFolders = allFolders.Where(f => f.EndsWith("\\target")).ToList();

            foreach (var binFolder in targetFolders)
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
