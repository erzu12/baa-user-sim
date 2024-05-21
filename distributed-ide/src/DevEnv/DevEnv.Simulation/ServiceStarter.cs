using System.Reflection;
using DevEnv.Base.Processes;

namespace DevEnv.Simulation
{
    /// <summary>
    /// Starts the known service components -- WorkDir Service, Build Service, Execute Serivce -- as separate processes for simulation/testing purposes.
    /// </summary>
    public class ServiceStarter : IDisposable
    {
        private readonly ProcessManager processManager = new ProcessManager();

        /// <summary>
        /// Starts the service components.
        /// </summary>
        public void StartServices()
        {
            var assemblyDir = GetExecutingAssemblyDirectory();
            var files = Directory.GetFiles(assemblyDir);

            var workDirServiceExeFile = files.Single(f => f.EndsWith("WorkDir.Service.exe"));
            var buildServiceExeFile = files.Single(f => f.EndsWith("Build.Service.exe"));
            var executeServiceExeFile = files.Single(f => f.EndsWith("Execute.Service.exe"));

            this.processManager.Start(assemblyDir!, workDirServiceExeFile, "--urls \"http://localhost:5188;https://localhost:7188\"", useShellExecute: true);
            this.processManager.Start(assemblyDir!, buildServiceExeFile, "--urls \"http://localhost:5288;https://localhost:7288\"", useShellExecute: true);
            this.processManager.Start(assemblyDir!, executeServiceExeFile, "--urls \"http://localhost:5388;https://localhost:7388\"", useShellExecute: true);
        }

        public void Dispose()
        {
            this.processManager.Dispose();
        }

        private static string GetExecutingAssemblyDirectory()
        {
            var codeBase = Assembly.GetExecutingAssembly().Location;
            var uri = new UriBuilder(codeBase);
            var assemblyPath = Uri.UnescapeDataString(uri.Path);
            var directoryPath = Path.GetDirectoryName(assemblyPath);

            if (string.IsNullOrEmpty(directoryPath))
            {
                throw new InvalidOperationException("The executing assembly directory could not be determined.");
            }

            return directoryPath;
        }
    }
}
