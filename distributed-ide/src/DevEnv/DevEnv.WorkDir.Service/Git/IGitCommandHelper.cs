namespace DevEnv.WorkDir.Service.Git
{
    /// <summary>
    /// Provides a simple interface for perfoming git commands.
    /// </summary>
    public interface IGitCommandHelper
    {
        /// <summary>
        /// Executes a "git clone" command.
        /// </summary>
        /// <param name="directory">
        /// The target directory
        /// </param>
        /// <param name="repoUrl">
        /// The URL of the repo to clone
        /// </param>
        /// <returns>
        /// A result indicating whether or not the command was success
        /// </returns>
        Task<GitResult> GitClone(string directory, string repoUrl);

        /// <summary>
        /// Executes a "git add" command.
        /// </summary>
        /// <param name="repoDirectory">
        /// The repo root directory
        /// </param>
        /// <param name="filePath">
        /// The path of the file to add
        /// </param>
        /// <returns>
        /// A result indicating whether or not the command was success
        /// </returns>
        Task<GitResult> GitAdd(string repoDirectory, string filePath);

        /// <summary>
        /// Executes a "git commit" command.
        /// </summary>
        /// <param name="repoDirectory">
        /// The repo root directory
        /// </param>
        /// <param name="commitMessage">
        /// The commit message
        /// </param>
        /// <returns>
        /// A result indicating whether or not the command was success
        /// </returns>
        Task<GitResult> GitCommit(string repoDirectory, string commitMessage);

        /// <summary>
        /// Executes a "git push" command.
        /// </summary>
        /// <param name="repoDirectory">
        /// The repo root directory
        /// </param>
        /// <param name="upstreamBranchName">
        /// The upstream branch name
        /// </param>
        /// <returns>
        /// A result indicating whether or not the command was success
        /// </returns>
        Task<GitResult> GitPush(string repoDirectory, string upstreamBranchName);

        /// <summary>
        /// Executes any git command.
        /// </summary>
        /// <param name="directory">
        /// The command working directory
        /// </param>
        /// <param name="command">
        /// The actual command
        /// </param>
        /// <returns>
        /// A result indicating whether or not the command was success
        /// </returns>
        Task<GitResult> ExecuteGitCommand(string directory, string command);
    }
}
