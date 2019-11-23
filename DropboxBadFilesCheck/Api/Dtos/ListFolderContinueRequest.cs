using System.Text.Json.Serialization;

namespace DropboxBadFilesCheck.Api.Dtos
{
    public class ListFolderContinueRequest : ApiRequest<ListFolderContinueResponse>
    {
        public ListFolderContinueRequest(string cursor)
        {
            Cursor = cursor;
        }

        [JsonPropertyName("cursor")]
        public string Cursor { get; set; }
    }
}