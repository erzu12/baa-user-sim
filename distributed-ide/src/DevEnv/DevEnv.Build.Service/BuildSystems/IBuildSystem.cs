namespace DevEnv.Build.Service.BuildSystems
{
    /// <summary>
    /// Provides an abstract interface for any specific build system.
    /// </summary>
    public interface IBuildSystem
    {
        /// <summary>
        /// Builds code, based on the specified build definition root file, using the specified working directory.
        /// </summary>
        /// <param name="workingDirectory">
        /// The working directory for the build process
        /// </param>
        /// <param name="rootFilePath">
        /// The root file containing the build definition
        /// </param>
        /// <returns>
        /// A build result
        /// </returns>
        Task<BuildResult> Build(string workingDirectory, string rootFilePath);

        /// <summary>
        /// Copies the relevant build artifacts (specific to a build system) from the specified build directory to a specified artifacts directory.
        /// </summary>
        /// <param name="buildDir">
        /// The directory containing the build artifacts prior to copying
        /// </param>
        /// <param name="artifactsDir">
        /// The directory to copy the artifacts to
        /// </param>
        void CopyBuildArtifacts(string buildDir, string artifactsDir);
    }
}
