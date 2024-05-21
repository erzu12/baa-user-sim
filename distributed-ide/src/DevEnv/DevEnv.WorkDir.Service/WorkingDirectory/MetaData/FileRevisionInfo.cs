using System.Text.Json.Serialization;

namespace DevEnv.WorkDir.Service.WorkingDirectory.MetaData
{
    /// <summary>
    /// Specifies the current revision of a specific file.
    /// </summary>
    public class FileRevisionInfo
    {
        [JsonPropertyName("filePath")]
        public string? FilePath { get; set; }

        [JsonPropertyName("currentRevisionNumber")]
        public int? CurrentRevisionNumber { get; set; }
    }
}
