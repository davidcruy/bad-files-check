using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace DropboxBadFilesCheck.Api.Dtos
{
    public class ListFolderResponse
    {
        [JsonPropertyName("entries")]
        public IList<FileEntry> Entries { get; set; }

        [JsonPropertyName("has_more")]
        public bool HasMore { get; set; }

        [JsonPropertyName("cursor")]
        public string Cursor { get; set; }
    }
}