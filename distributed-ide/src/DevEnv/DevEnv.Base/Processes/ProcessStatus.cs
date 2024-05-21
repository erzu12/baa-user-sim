
namespace DevEnv.Base.Processes
{
    /// <summary>
    /// Represents the (simplified) status of process.
    /// </summary>
    public class ProcessStatus
    {
        public ProcessStatus(
            bool hasExited,
            int? exitCode)
        {
            this.HasExited = hasExited;
            this.ExitCode = exitCode;
        }

        /// <summary>
        /// Gets a value indicating whether the process has already exited.
        /// </summary>
        public bool HasExited { get; }

        /// <summary>
        /// Gets the process exit code (only relevant if the process has already exited.
        /// </summary>
        public int? ExitCode { get; }
    }
}
