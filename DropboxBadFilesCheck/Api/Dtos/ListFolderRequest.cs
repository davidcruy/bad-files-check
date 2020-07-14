using System.Text.Json.Serialization;

namespace DropboxBadFilesCheck.Api.Dtos
{
    public class ListFolderRequest : ApiRequest<ListFolderResponse>
    {
        public ListFolderRequest(string path, bool recursive)
        {
            Path = path;
            Recursive = recursive;
            IncludeMediaInfo = false;
            IncludeDeleted = false;
            IncludeHasExplicitSharedMembers = false;
            IncludeMountedFolders = true;
            IncludeNonDownloadableFiles = true;
        }

        [JsonPropertyName("path")]
        public string Path { get; set; }
        [JsonPropertyName("recursive")]
        public bool Recursive { get; set; }
        [JsonPropertyName("include_media_info")]
        public bool IncludeMediaInfo { get; set; }
        [JsonPropertyName("include_deleted")]
        public bool IncludeDeleted { get; set; }
        [JsonPropertyName("include_has_explicit_shared_members")]
        public bool IncludeHasExplicitSharedMembers { get; set; }
        [JsonPropertyName("include_mounted_folders")]
        public bool IncludeMountedFolders { get; set; }
        [JsonPropertyName("include_non_downloadable_files")]
        public bool IncludeNonDownloadableFiles { get; set; }
    }
}