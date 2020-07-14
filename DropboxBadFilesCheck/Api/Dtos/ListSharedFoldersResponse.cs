using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace DropboxBadFilesCheck.Api.Dtos
{
    public class ListSharedFoldersResponse
    {
        [JsonPropertyName("entries")]
        public IList<SharedFolderMetaData> Entries { get; set; }

        [JsonPropertyName("cursor")]
        public string Cursor { get; set; }
    }
}