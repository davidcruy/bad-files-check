using System.Text.Json.Serialization;

namespace DropboxBadFilesCheck.Api.Dtos
{
    public class MoveRequest : ApiRequest<MoveResponse>
    {
        public MoveRequest(string fromPath, string toPath)
        {
            FromPath = fromPath;
            ToPath = toPath;
            AutoRename = true;
            AllowOwnershipTransfer = false;
        }

        [JsonPropertyName("from_path")]
        public string FromPath { get; set; }

        [JsonPropertyName("to_path")]
        public string ToPath { get; set; }

        [JsonPropertyName("allow_shared_folder")]
        public bool AllowSharedFolder { get; set; }

        [JsonPropertyName("autorename")]
        public bool AutoRename { get; set; }

        [JsonPropertyName("allow_ownership_transfer")]
        public bool AllowOwnershipTransfer { get; set; }
    }
}