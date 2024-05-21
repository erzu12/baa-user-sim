using DevEnv.Base.Processes;

namespace DevEnv.Execute.Service.Execution.Java
{
    /// <summary>
    /// Executes a Java Archive (JAR file).
    /// </summary>
    public class JavaJarExecutor : IExecute
    {
        public string StartExecute(string workingDirectory, string rootFilePath, IProcessManager processManager)
        {
            var executeId = processManager.Start(
                workingDirectory,
                "java",
                arguments: rootFilePath);

            return executeId;
        }
    }
}
