using DevEnv.Base.FileSystem;
using DevEnv.Base.RuntimeChecks;
using DevEnv.Base.Settings;
using DevEnv.Build.Service.BuildArtifacts.MetaData;

namespace DevEnv.Build.Service.BuildArtifacts
{
    public class BuildArtifactsHelper : IBuildArtifactsHelper
    {
        private readonly ISettings settings;
        private readonly IMetadataFileUtils metadataFileUtils;

        public BuildArtifactsHelper(ISettingsProvider<ISettings> settingsProvider, IMetadataFileUtils metadataFileUtils)
        {
            Argument.AssertNotNull(settingsProvider, nameof(settingsProvider));
            Argument.AssertNotNull(metadataFileUtils, nameof(metadataFileUtils));

            this.settings = settingsProvider.GetSettings();
            this.metadataFileUtils = metadataFileUtils;
        }

        public void UpdatePersistTotalArtifactsMetadata(string workDirId, Action<BuildArtifactsMetadata> updateMetadataAction)
        {
            Argument.AssertNotEmpty(workDirId, nameof(workDirId));

            var artifactsBaseDir = this.GetArtifactsBaseDirectoryPath(workDirId);
            var artifactsBaseDirMetadataFilePath = Path.Combine(artifactsBaseDir, "artifacts-total.json");

            this.metadataFileUtils.UpdatePersistMetadata(artifactsBaseDirMetadataFilePath, updateMetadataAction);
        }

        public BuildArtifactsMetadata GetTotalArtifactsMetadata(string workDirId)
        {
            Argument.AssertNotEmpty(workDirId, nameof(workDirId));

            var artifactsBaseDir = this.GetArtifactsBaseDirectoryPath(workDirId);
            var artifactsBaseDirMetadataFilePath = Path.Combine(artifactsBaseDir, "artifacts-total.json");

            var metadata = this.metadataFileUtils.GetMetadata<BuildArtifactsMetadata>(artifactsBaseDirMetadataFilePath);

            return metadata;
        }

        public void UpdatePersistBuildSpecificArtifactsMetadata(string workDirId, string buildId, Action<BuildArtifactsMetadata> updateMetadataAction)
        {
            Argument.AssertNotEmpty(workDirId, nameof(workDirId));
            Argument.AssertNotEmpty(buildId, nameof(buildId));
            Argument.AssertNotNull(updateMetadataAction, nameof(updateMetadataAction));

            var artifactsBaseDir = this.GetArtifactsBaseDirectoryPath(workDirId);
            var buildSpecificMetadataFilePath = Path.Combine(artifactsBaseDir, $"artifacts-{buildId}.json");

            this.metadataFileUtils.UpdatePersistMetadata(buildSpecificMetadataFilePath, updateMetadataAction);
        }

        public BuildArtifactsMetadata GetBuildSpecificArtifactsMetadata(string workDirId, string buildId)
        {
            var artifactsBaseDir = this.GetArtifactsBaseDirectoryPath(workDirId);
            var buildSpecificMetadataFilePath = Path.Combine(artifactsBaseDir, $"artifacts-{buildId}.json");

            return this.metadataFileUtils.GetMetadata<BuildArtifactsMetadata>(buildSpecificMetadataFilePath);
        }

        public string GetCacheDirectoryPath(string workDirId)
        {
            Argument.AssertNotEmpty(workDirId, nameof(workDirId));

            return Path.Combine(this.settings.DataDirectoryPath, workDirId, "cache");
        }

        public string GetBuildBaseDirectoryPath(string workDirId)
        {
            Argument.AssertNotEmpty(workDirId, nameof(workDirId));

            return Path.Combine(this.settings.DataDirectoryPath, workDirId, "build");
        }

        public string GetArtifactsBaseDirectoryPath(string workDirId)
        {
            Argument.AssertNotEmpty(workDirId, nameof(workDirId));

            return Path.Combine(this.settings.DataDirectoryPath, workDirId, "artifacts");
        }

        public string GetBuildArtifactsDirectoryPath(string workDirId, string buildId)
        {
            Argument.AssertNotEmpty(workDirId, nameof(workDirId));
            Argument.AssertNotEmpty(buildId, nameof(buildId));

            var baseDir = this.GetArtifactsBaseDirectoryPath(workDirId);
            var buildArtifactsDir = Path.Combine(baseDir, buildId);

            return buildArtifactsDir;
        }
    }
}
