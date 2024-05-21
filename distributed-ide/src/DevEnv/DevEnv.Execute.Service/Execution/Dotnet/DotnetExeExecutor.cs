using DevEnv.Base.Processes;

namespace DevEnv.Execute.Service.Execution.Dotnet
{
    /// <summary>
    /// Executes a .NET executable (EXE file).
    /// </summary>
    public class DotnetExeExecutor : IExecute
    {
        public string StartExecute(string workingDirectory, string rootFilePath, IProcessManager processManager)
        {
            var executeId = processManager.Start(
                workingDirectory,
                rootFilePath,
                arguments: "");

            return executeId;
        }
    }
}
