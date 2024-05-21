using System.Text.Json.Serialization;

namespace DevEnv.Build.Service.BuildArtifacts.MetaData
{
    /// <summary>
    /// Specifies, for a specific build artifact file, which build the artifact originates from
    /// and which revision it is currently in.
    /// </summary>
    public class BuildArtifactInfo
    {
        [JsonPropertyName("filePath")]
        public string? FilePath { get; set; }

        [JsonPropertyName("buildId")]
        public string? BuildId { get; set; }

        [JsonPropertyName("currentRevisionNumber")]
        public int? CurrentRevisionNumber { get; set; }
    }
}
