using DevEnv.Base.Processes;
using DevEnv.Base.RuntimeChecks;

namespace DevEnv.WorkDir.Service.Git
{
    public class GitCommandHelper : IGitCommandHelper
    {
        private readonly ILogger<GitCommandHelper> logger;

        public GitCommandHelper(ILogger<GitCommandHelper> logger)
        {
            Argument.AssertNotNull(logger, nameof(logger));

            this.logger = logger;
        }

        public async Task<GitResult> GitClone(string directory, string repoUrl)
        {
            Argument.AssertNotEmpty(directory, nameof(directory));
            Argument.AssertNotEmpty(repoUrl, nameof(repoUrl));

            return await this.ExecuteGitCommand(directory, $"clone {repoUrl}");
        }

        public async Task<GitResult> GitAdd(string repoDirectory, string filePath)
        {
            Argument.AssertNotEmpty(repoDirectory, nameof(repoDirectory));
            Argument.AssertNotEmpty(filePath, nameof(filePath));

            return await this.ExecuteGitCommand(repoDirectory, $"add \"{filePath}\"");
        }

        public async Task<GitResult> GitCommit(string repoDirectory, string commitMessage)
        {
            Argument.AssertNotEmpty(repoDirectory, nameof(repoDirectory));
            Argument.AssertNotEmpty(commitMessage, nameof(commitMessage));

            var output = await this.ExecuteGitCommand(repoDirectory, $"commit -m \"{commitMessage}\"");

            return output;
        }

        public async Task<GitResult> GitPush(string repoDirectory, string upstreamBranchName)
        {
            Argument.AssertNotEmpty(repoDirectory, nameof(repoDirectory));
            Argument.AssertNotEmpty(upstreamBranchName, nameof(upstreamBranchName));

            var output = await this.ExecuteGitCommand(repoDirectory, $"push -u origin \"{upstreamBranchName}\"");

            return output;
        }

        public async Task<GitResult> ExecuteGitCommand(string directory, string command)
        {
            Argument.AssertNotEmpty(directory, nameof(directory));
            Argument.AssertNotEmpty(command, nameof(command));

            this.logger.LogInformation($"git {command}");

            using var processManager = new ProcessManager();
            var processId = processManager.Start(directory, "git", command);

            var exitCode = await processManager.Wait(processId);

            var output = processManager.GetCompleteOutput(processId);
            this.logger.LogInformation(output);

            return new GitResult(exitCode == 0, output);
        }
    }
}
