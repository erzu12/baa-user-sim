namespace DevEnv.WorkDir.Service.Git
{
    /// <summary>
    /// Represents the result of a git command execution.
    /// </summary>
    public class GitResult
    {
        public GitResult(bool successful, string output)
        {
            this.Successful = successful;
            this.Output = output;
        }

        /// <summary>
        /// Gets a value indicating whether the git command execution was successful.
        /// </summary>
        public bool Successful { get; }

        /// <summary>
        /// Gets the output text.
        /// </summary>
        public string Output { get; }
    }
}
