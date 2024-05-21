using DevEnv.Base.Settings;
using DevEnv.Build.Client;
using DevEnv.Execute.Client;
using DevEnv.WorkDir.Client;

namespace DevEnv.Simulation
{
    /// <summary>
    /// Provides a simple interface -- for simulation/testing -- to interact with the service components like a real code editor might.
    /// </summary>
    public class MockEditor : IDisposable
    {
        private readonly RemoteWorkDirService workDirService;
        private readonly RemoteBuildService buildService;
        private readonly RemoteExecuteService executeService;

        public MockEditor()
            : this(
                  "https://localhost:7188",
                  "https://localhost:7288",
                  "https://localhost:7388")
        {
        }

        public MockEditor(
            string workDirServiceAddress,
            string buildServiceAddress,
            string executeServiceAddress)
        {
            var workDirClientSettings = new WorkDir.Client.Settings
            {
                WorkDirServiceAddress = workDirServiceAddress
            };

            var buildClientSettings = new Build.Client.Settings
            {
                BuildServiceAddress = buildServiceAddress,
            };

            var executeClientSettings = new Execute.Client.Settings
            {
                ExecuteServiceAddress = executeServiceAddress
            };

            this.workDirService = new RemoteWorkDirService(new InMemorySettingsProvider<WorkDir.Client.ISettings>(() => workDirClientSettings));
            this.buildService = new RemoteBuildService(new InMemorySettingsProvider<Build.Client.ISettings>(() => buildClientSettings));
            this.executeService = new RemoteExecuteService(new InMemorySettingsProvider<Execute.Client.ISettings>(() => executeClientSettings));
        }

        /// <summary>
        /// Gets the <see cref="IWorkDirService"/>.
        /// </summary>
        public IWorkDirService WorkDir => this.workDirService;

        /// <summary>
        /// Gets the <see cref="IBuildService"/>.
        /// </summary>
        public IBuildService Build => this.buildService;

        /// <summary>
        /// Gets the <see cref="IExecuteService"/>.
        /// </summary>
        public IExecuteService Execute => this.executeService;

        public void Dispose()
        {
            this.workDirService.Dispose();
            this.buildService.Dispose();
            this.executeService.Dispose();
        }
    }
}