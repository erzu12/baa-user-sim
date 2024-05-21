using DevEnv.Build.Service.BuildSystems.Dotnet;
using DevEnv.Build.Service.BuildSystems.Maven;
using GrpcBuild;

namespace DevEnv.Build.Service.BuildSystems
{
    public class BuildSystemProvider : IBuildSystemProvider
    {
        private readonly DotnetBuildSystem dotnetStrategy;
        private readonly MavenBuildSystem mavenStrategy;

        public BuildSystemProvider(DotnetBuildSystem dotnetStrategy, MavenBuildSystem mavenStrategy)
        {
            this.dotnetStrategy = dotnetStrategy;
            this.mavenStrategy = mavenStrategy;
        }

        public IBuildSystem GetSpecificBuildStrategy(BuildSystem buildSystem)
        {
            switch (buildSystem)
            {
                case BuildSystem.Dotnet:
                    return this.dotnetStrategy;
                case BuildSystem.Maven:
                    return this.mavenStrategy;
                default:
                    throw new NotSupportedException($"The build system {buildSystem} is not supported.");
            }
        }
    }
}
