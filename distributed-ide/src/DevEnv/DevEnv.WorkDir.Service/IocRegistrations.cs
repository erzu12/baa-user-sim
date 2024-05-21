using DevEnv.Base.FileSystem;
using DevEnv.Base.Ioc;
using DevEnv.Base.Settings;
using DevEnv.WorkDir.Service.Git;
using DevEnv.WorkDir.Service.Services;
using DevEnv.WorkDir.Service.WorkingDirectory;

namespace DevEnv.WorkDir.Service
{
    /// <summary>
    /// Performs the necessary IOC registrations for this entire component.
    /// </summary>
    public static class IocRegistrations
    {
        public static void DoRegistrations(IIocRegistry registry)
        {
            _ = registry
                .RegisterSingletonInstance<ISettingsProvider<ISettings>>(new SettingsFileLoader<Settings>("workdir-service-settings.json"))
                .RegisterSingleton<IWorkDirHelper, WorkDirHelper>()
                .RegisterSingleton<IMetadataFileUtils, MetadataFileUtils>()
                .RegisterSingleton<IGitCommandHelper, GitCommandHelper>()
                .RegisterSingleton<WorkDirService, WorkDirService>();
        }
    }
}
