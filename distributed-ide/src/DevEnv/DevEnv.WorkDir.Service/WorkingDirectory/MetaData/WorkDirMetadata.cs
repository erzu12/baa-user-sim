using System.Text.Json.Serialization;

namespace DevEnv.WorkDir.Service.WorkingDirectory.MetaData
{
    /// <summary>
    /// Contains all the metadata for a specific working directory.
    /// </summary>
    public class WorkDirMetadata
    {
        [JsonPropertyName("id")]
        public string? Id { get; set; }

        [JsonPropertyName("description")]
        public string? Description { get; set; }

        [JsonPropertyName("repoUrl")]
        public string? RepoUrl { get; set; }

        [JsonPropertyName("branch")]
        public string? Branch { get; set; }

        [JsonPropertyName("repoDirName")]
        public string? RepoDirName { get; set; }

        [JsonPropertyName("fileRevisions")]
        public List<FileRevisionInfo> FileRevisions { get; set; } = new List<FileRevisionInfo>();
    }
}
