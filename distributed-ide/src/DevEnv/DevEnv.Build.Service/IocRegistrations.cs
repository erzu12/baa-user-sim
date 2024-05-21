using DevEnv.Base.Caching;
using DevEnv.Base.FileSystem;
using DevEnv.Base.Ioc;
using DevEnv.Base.Settings;
using DevEnv.Build.Service.BuildArtifacts;
using DevEnv.Build.Service.BuildSystems;
using DevEnv.Build.Service.BuildSystems.Dotnet;
using DevEnv.Build.Service.BuildSystems.Maven;
using DevEnv.Build.Service.Services;
using DevEnv.WorkDir.Client;

namespace DevEnv.Build.Service
{
    /// <summary>
    /// Performs the necessary IOC registrations for this entire component.
    /// </summary>
    public class IocRegistrations
    {
        public static void DoRegistrations(IIocRegistry registry)
        {
            var settingsFileLoader = new SettingsFileLoader<Settings>("build-service-settings.json");
            _ = registry
                .RegisterSingletonInstance<ISettingsProvider<ISettings>>(settingsFileLoader)
                .RegisterSingletonInstance<ISettingsProvider<WorkDir.Client.ISettings>>(settingsFileLoader)
                .RegisterSingleton<IBuildArtifactsHelper, BuildArtifactsHelper>()
                .RegisterSingleton<IMetadataFileUtils, MetadataFileUtils>()
                .RegisterSingleton<IWorkDirService, RemoteWorkDirService>()
                .RegisterSingleton<IFileCacheManager, FileCacheManager>()
                .RegisterSingleton<IFileSystemUtils, FileSystemUtils>()
                .RegisterSingleton<IBuildSystemProvider, BuildSystemProvider>()
                .RegisterSingleton<DotnetBuildSystem, DotnetBuildSystem>()
                .RegisterSingleton<MavenBuildSystem, MavenBuildSystem>()
                .RegisterSingleton<BuildService, BuildService>();
        }
    }
}
