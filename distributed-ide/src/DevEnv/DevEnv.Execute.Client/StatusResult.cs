namespace DevEnv.Execute.Client
{
    /// <summary>
    /// Describes the status of an execution/process.
    /// </summary>
    public class StatusResult
    {
        public StatusResult(
            bool hasExited,
            int exitCode)
        {
            this.HasExited = hasExited;
            this.ExitCode = exitCode;
        }

        /// <summary>
        /// Gets a value indicating whether the process has already exited.
        /// </summary>
        public bool HasExited { get; }

        /// <summary>
        /// Gets the exit code (only relevant if the process has already exited).
        /// </summary>
        public int ExitCode { get; }
    }
}
