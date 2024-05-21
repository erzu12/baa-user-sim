namespace DevEnv.Build.Service.BuildSystems
{
    /// <summary>
    /// Represents the result of a build process.
    /// </summary>
    public class BuildResult
    {
        public BuildResult(bool successful, string output)
        {
            this.Successful = successful;
            this.Output = output;
        }

        /// <summary>
        /// Gets a value indicating whether the build process was successful.
        /// </summary>
        public bool Successful { get; }

        /// <summary>
        /// Gets the process output text.
        /// </summary>
        public string Output { get; }
    }
}