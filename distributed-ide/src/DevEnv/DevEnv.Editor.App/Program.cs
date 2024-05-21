// See https://aka.ms/new-console-template for more information
using DevEnv.Base.Settings;
using DevEnv.Build.Client;
using DevEnv.Editor.Controls;
using DevEnv.Execute.Client;
using DevEnv.WorkDir.Client;

namespace DevEnv.Editor.App
{
    public class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            Console.WriteLine("Editor App");

            var workDirClientSettings = new WorkDir.Client.Settings { WorkDirServiceAddress = "https://localhost:7188" };
            var buildClientSettings = new Build.Client.Settings { BuildServiceAddress = "https://localhost:7288" };
            var executeClientSettings = new Execute.Client.Settings { ExecuteServiceAddress = "https://localhost:7388" };

            var workDirService = new RemoteWorkDirService(new InMemorySettingsProvider<WorkDir.Client.ISettings>(() => workDirClientSettings));
            var buildService = new RemoteBuildService(new InMemorySettingsProvider<Build.Client.ISettings>(() => buildClientSettings));
            var executeService = new RemoteExecuteService(new InMemorySettingsProvider<Execute.Client.ISettings>(() => executeClientSettings));

            var window = new MainWindow(workDirService, buildService, executeService);

            window.ShowDialog();
        }
    }
}


