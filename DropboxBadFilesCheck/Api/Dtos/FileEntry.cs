using System.Text.Json.Serialization;

namespace DropboxBadFilesCheck.Api.Dtos
{
    public class FileEntry
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName(".tag")]
        public string Tag { get; set; }

        [JsonPropertyName("path_lower")]
        public string PathLower { get; set; }
    }
}