using System.Text.Json.Serialization;

namespace DevEnv.Build.Service.BuildArtifacts.MetaData
{
    /// <summary>
    /// Contains an index of build artifacts and their current revisions.
    /// </summary>
    public class BuildArtifactsMetadata
    {
        [JsonPropertyName("artifacts")]
        public List<BuildArtifactInfo> Artifacts { get; set; } = new List<BuildArtifactInfo>();
    }
}
