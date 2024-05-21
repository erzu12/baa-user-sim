using DevEnv.Base.Caching;
using DevEnv.Base.FileSystem;
using DevEnv.Base.Ioc;
using DevEnv.Base.Processes;
using DevEnv.Base.Settings;
using DevEnv.Build.Client;
using DevEnv.Execute.Service.Execution;
using DevEnv.Execute.Service.Services;

namespace DevEnv.Execute.Service
{
    /// <summary>
    /// Performs the necessary IOC registrations for this entire component.
    /// </summary>
    public class IocRegistrations
    {
        public static void DoRegistrations(IIocRegistry registry)
        {
            var settingsFileLoader = new SettingsFileLoader<Settings>("execute-service-settings.json");
            _ = registry
                .RegisterSingletonInstance<ISettingsProvider<ISettings>>(settingsFileLoader)
                .RegisterSingletonInstance<ISettingsProvider<Build.Client.ISettings>>(settingsFileLoader)
                .RegisterSingleton<IFileSystemUtils, FileSystemUtils>()
                .RegisterSingleton<IBuildService, RemoteBuildService>()
                .RegisterSingleton<IFileCacheManager, FileCacheManager>()
                .RegisterSingleton<IFileSystemUtils, FileSystemUtils>()
                .RegisterSingleton<IExecutorProvider, ExecutorProvider>()
                .RegisterSingleton<IProcessManager, ProcessManager>()
                .RegisterSingleton<ExecuteService, ExecuteService>();
        }
    }
}
