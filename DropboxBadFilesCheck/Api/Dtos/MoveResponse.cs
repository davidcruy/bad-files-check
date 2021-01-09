using System.Text.Json.Serialization;

namespace DropboxBadFilesCheck.Api.Dtos
{
    public class MoveResponse
    {
        [JsonPropertyName("metadata")]
        public FileEntry MetaData { get; set; }

        [JsonPropertyName("error_summary")]
        public string ErrorSummary { get; set; }
    }
}