using DevEnv.Base.RuntimeChecks;
using System.Text.Json;

namespace DevEnv.Base.FileSystem
{
    public class MetadataFileUtils : IMetadataFileUtils
    {
        public void UpdatePersistMetadata<TMetadata>(string metadataFilePath, Action<TMetadata> updateMetadataAction)
            where TMetadata : class, new()
        {
            Argument.AssertNotEmpty(metadataFilePath, nameof(metadataFilePath));
            Argument.AssertNotNull(updateMetadataAction, nameof(updateMetadataAction));

            // Create new or load existing.
            var metadataToStore = new TMetadata();

            if (File.Exists(metadataFilePath))
            {
                var existingMetadataJson = File.ReadAllText(metadataFilePath);
                metadataToStore = JsonSerializer.Deserialize<TMetadata>(existingMetadataJson)!;
            }

            // Callback to update metadata.
            updateMetadataAction(metadataToStore);

            // Persist.
            var newMetadataJson = JsonSerializer.Serialize(metadataToStore);
            File.WriteAllText(metadataFilePath, newMetadataJson);
        }

        public TMetadata GetMetadata<TMetadata>(string metadataFilePath)
            where TMetadata : class, new()
        {
            Argument.AssertNotEmpty(metadataFilePath, nameof(metadataFilePath));

            if (!File.Exists(metadataFilePath))
            {
                return new TMetadata();
            }

            var metadataJson = File.ReadAllText(metadataFilePath);
            var metadata = JsonSerializer.Deserialize<TMetadata>(metadataJson);

            if (metadata == null)
            {
                throw new InvalidOperationException($"The metadata file \"{metadataFilePath}\" could not deserialized.");
            }

            return metadata;
        }
    }
}
