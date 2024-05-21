
namespace DevEnv.Base.FileSystem
{
    /// <summary>
    /// Helps (hopefully) with storing and retrieving metadata to and from metadata files.
    /// </summary>
    public interface IMetadataFileUtils
    {
        /// <summary>
        /// Updates and persists the specified metadata file. If it doesn't already exist, it is created.
        /// </summary>
        /// <typeparam name="TMetadata">
        /// The metadata type
        /// </typeparam>
        /// <param name="metadataFilePath">
        /// The file where the metadata is stored
        /// </param>
        /// <param name="updateMetadataAction">
        /// An action which updates the metadata object
        /// </param>
        void UpdatePersistMetadata<TMetadata>(string metadataFilePath, Action<TMetadata> updateMetadataAction)
            where TMetadata : class, new();

        /// <summary>
        /// Gets the metadata from the specified metadata file.
        /// </summary>
        /// <typeparam name="TMetadata">
        /// The metadata type
        /// </typeparam>
        /// <param name="metadataFilePath">
        /// The file where the metadata is stored
        /// </param>
        /// <returns>
        /// The deserialized metadata object
        /// </returns>
        TMetadata GetMetadata<TMetadata>(string metadataFilePath)
            where TMetadata : class, new();
    }
}
