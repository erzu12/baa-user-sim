using GrpcBuild;

namespace DevEnv.Build.Service.BuildSystems
{
    /// <summary>
    /// Provides specific implementations for each supported build system.
    /// </summary>
    public interface IBuildSystemProvider
    {
        /// <summary>
        /// Gets the specific build system implementation for the specified enum value.
        /// </summary>
        /// <param name="buildSystem">
        /// The build system enum value
        /// </param>
        /// <returns>
        /// The build system implementation
        /// </returns>
        IBuildSystem GetSpecificBuildStrategy(BuildSystem buildSystem);
    }
}
