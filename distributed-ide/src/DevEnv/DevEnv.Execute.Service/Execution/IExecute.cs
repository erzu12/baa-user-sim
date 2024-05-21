using DevEnv.Base.Processes;

namespace DevEnv.Execute.Service.Execution
{
    /// <summary>
    /// Provides an abstract interface for executing different types of build artifacts.
    /// </summary>
    public interface IExecute
    {
        /// <summary>
        /// Starts executing the specified file.
        /// </summary>
        /// <param name="workingDirectory">
        /// The process working directory
        /// </param>
        /// <param name="rootFilePath">
        /// The file to execute
        /// </param>
        /// <param name="processManager">
        /// The process manager instance
        /// </param>
        /// <returns>
        /// The execution ID
        /// </returns>
        string StartExecute(string workingDirectory, string rootFilePath, IProcessManager processManager);
    }
}
