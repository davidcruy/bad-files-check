using System.Text.Json.Serialization;

namespace DropboxBadFilesCheck.Api.Dtos
{
    public class ListSharedFoldersContinueRequest : ApiRequest<ListSharedFoldersContinueResponse>
    {
        public ListSharedFoldersContinueRequest(string cursor)
        {
            Cursor = cursor;
        }

        [JsonPropertyName("cursor")]
        public string Cursor { get; }
    }
}