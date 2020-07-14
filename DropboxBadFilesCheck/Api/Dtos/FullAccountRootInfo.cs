using System.Text.Json.Serialization;

namespace DropboxBadFilesCheck.Api.Dtos
{
    public class FullAccountRootInfo
    {
        [JsonPropertyName(".tag")]
        public string Tag { get; set; }

        [JsonPropertyName("root_namespace_id")]
        public string RootNamespaceId { get; set; }

        [JsonPropertyName("home_hamespace_id")]
        public string HomeNamespaceId { get; set; }
    }
}