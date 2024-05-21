namespace DevEnv.Execute.Service.Execution
{
    /// <summary>
    /// Provides the right executor for each environment / file type.
    /// </summary>
    public interface IExecutorProvider
    {
        /// <summary>
        /// Gets the right executor for the specified environment / file type.
        /// </summary>
        /// <param name="env">
        /// The environment / file type
        /// </param>
        /// <returns>
        /// The executor
        /// </returns>
        IExecute GetExecutor(GrpcExecute.Environment env);
    }
}
