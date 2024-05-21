
namespace DevEnv.Base.Processes
{
    /// <summary>
    /// Manages mutliple processes.
    /// </summary>
    public interface IProcessManager
    {
        /// <summary>
        /// Starts a process based on the specified info, and returns a unique execution ID.
        /// </summary>
        /// <param name="workingDirectory">
        /// The process working directory
        /// </param>
        /// <param name="fileName">
        /// The file to execute
        /// </param>
        /// <param name="arguments">
        /// The arguments passed to the process
        /// </param>
        /// <returns>
        /// The execution ID of the new process
        /// </returns>
        string Start(string workingDirectory, string fileName, string arguments, bool useShellExecute = false);

        /// <summary>
        /// Gets the execution status.
        /// </summary>
        /// <param name="id">
        /// The execution ID
        /// </param>
        /// <returns>
        /// The execution status
        /// </returns>
        ProcessStatus GetStatus(string id);

        /// <summary>
        /// Gets all current output lines, starting from the specified offset.
        /// </summary>
        /// <param name="id">
        /// The execution ID
        /// </param>
        /// <param name="offset">
        /// The line offset
        /// </param>
        /// <returns>
        /// All current output lines starting from the offset
        /// </returns>
        IReadOnlyList<string> GetOutputLines(string id, int offset);

        /// <summary>
        /// Gets the complete output (up until now).
        /// </summary>
        /// <returns>
        /// The complete output (up until now)
        /// </returns>
        string GetCompleteOutput(string id);

        /// <summary>
        /// Waits until the process terminates, and returns the exit code.
        /// </summary>
        /// <param name="id">
        /// The execution ID
        /// </param>
        Task<int> Wait(string id);

        /// <summary>
        /// Kills the specified process.
        /// </summary>
        /// <param name="id">
        /// The execution ID
        /// </param>
        void Kill(string id);
    }
}
