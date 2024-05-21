namespace DevEnv.Execute.Client
{
    /// <summary>
    /// The client API for the Execute Service.
    /// </summary>
    public interface IExecuteService
    {
        /// <summary>
        /// Starts an execution process of a .NET executable from the specified working directory and build.
        /// </summary>
        /// <param name="workDirId">
        /// The working directory ID
        /// </param>
        /// <param name="buildId">
        /// The build ID
        /// </param>
        /// <param name="executeFile">
        /// The executable file, as relative path within the working directory
        /// </param>
        /// <returns>
        /// A unique execution ID
        /// </returns>
        Task<string> StartExecuteDotnet(string workDirId, string buildId, string executeFile);

        /// <summary>
        /// Starts an execution process of a .NET executable from the specified working directory and build.
        /// </summary>
        /// <param name="workDirId">
        /// The working directory ID
        /// </param>
        /// <param name="buildId">
        /// The build ID
        /// </param>
        /// <param name="environment">
        /// The runtime environment
        /// </param>
        /// <param name="executeFile">
        /// The executable file, as relative path within the working directory
        /// </param>
        /// <returns>
        /// A unique execution ID
        /// </returns>
        Task<string> StartExecute(string workDirId, string buildId, GrpcExecute.Environment environment, string executeFile);

        /// <summary>
        /// Gets the current status of the specified execution/process.
        /// </summary>
        /// <param name="executeId">
        /// The execution ID
        /// </param>
        /// <returns>
        /// The current status
        /// </returns>
        Task<StatusResult> GetStatus(string executeId);

        /// <summary>
        /// Gets the output lines, starting at the specified line offset, from the specified execution/process.
        /// </summary>
        /// <param name="executeId">
        /// The execution ID
        /// </param>
        /// <param name="offset">
        /// The output line offset
        /// </param>
        /// <returns>
        /// The output lines, starting at the specified line offset
        /// </returns>
        Task<IReadOnlyList<string>> GetOutputLines(string executeId, int offset);

        /// <summary>
        /// Kills the execution/process.
        /// </summary>
        /// <param name="executeId">
        /// The execution ID
        /// </param>
        /// <returns>
        /// An awaitable task
        /// </returns>
        Task Kill(string executeId);
    }
}