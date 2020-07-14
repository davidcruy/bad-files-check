using System.Text.Json.Serialization;

namespace DropboxBadFilesCheck.Api.Dtos
{
    public class SharedFolderMetaData
    {
        [JsonPropertyName("is_inside_team_folder")]
        public bool IsInsideTeamFolder { get; set; }

        [JsonPropertyName("is_team_folder")]
        public bool IsTeamFolder { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("path_lower")]
        public string PathLower { get; set; }

        [JsonPropertyName("shared_folder_id ")]
        public string SharedFolderId { get; set; }
    }
}