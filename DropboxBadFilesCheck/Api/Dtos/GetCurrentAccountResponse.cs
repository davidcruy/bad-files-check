using System.Text.Json.Serialization;

namespace DropboxBadFilesCheck.Api.Dtos
{
    public class GetCurrentAccountResponse
    {
        [JsonPropertyName("email")]
        public string Email { get; set; }

        [JsonPropertyName("root_info")]
        public FullAccountRootInfo RootInfo { get; set; }
    }
}