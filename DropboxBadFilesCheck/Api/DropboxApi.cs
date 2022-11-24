using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using DropboxBadFilesCheck.Api.Dtos;

namespace DropboxBadFilesCheck.Api
{
    internal class DropboxApi
    {
        private readonly HttpClient _client;
        private readonly CancellationToken _token;

        public DropboxApi(string token, CancellationToken cancellationToken)
        {
            _token = cancellationToken;

            _client = new HttpClient
            {
                BaseAddress = new Uri("https://api.dropboxapi.com/")
            };

            _client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");
        }

        public async Task<GetCurrentAccountResponse> GetCurrentAccount()
        {
            var getCurrentAccountResponse = await PerformRequest<GetCurrentAccountResponse>("2/users/get_current_account");

            return getCurrentAccountResponse;
        }

        public void SetPathRoot(string rootNamespaceId)
        {
            _client.DefaultRequestHeaders.Add("Dropbox-API-Path-Root", "{\".tag\": \"root\", \"root\": \"" + rootNamespaceId + "\"}");
        }

        public async IAsyncEnumerable<FileEntry> ListFolder(string path)
        {
            var listFolderRequest = new ListFolderRequest(path, true);
            var listFolderResponse = await PerformRequest<ListFolderRequest, ListFolderResponse>(listFolderRequest, "2/files/list_folder");

            if (listFolderResponse.Entries != null)
                foreach (var entry in listFolderResponse.Entries)
                    yield return entry;

            var hasMore = listFolderResponse.HasMore;
            var cursor = listFolderResponse.Cursor;
            var counter = 0;

            while (hasMore)
            {
                counter++;
                if (counter > 100000)
                {
                    throw new Exception("List folder request overflow!");
                }

                var continueRequest = new ListFolderContinueRequest(cursor);
                var continueResponse = await PerformRequest<ListFolderContinueRequest, ListFolderContinueResponse>(continueRequest, "2/files/list_folder/continue");

                if (continueResponse.Entries != null)
                    foreach (var entry in continueResponse.Entries)
                        yield return entry;

                hasMore = continueResponse.HasMore;
                cursor = continueResponse.Cursor;
            }
        }

        public async Task<string> Move(string fromPath, string toPath)
        {
            var moveRequest = new MoveRequest(fromPath, toPath);
            var moveResponse = await PerformRequest<MoveRequest, MoveResponse>(moveRequest, "2/files/move_v2");

            if (!string.IsNullOrEmpty(moveResponse.ErrorSummary))
            {
                throw new Exception($"Move files returned an error: {moveResponse.ErrorSummary}");
            }

            var path = moveResponse.MetaData?.PathLower;
            return path;
        }

        public async Task<IList<SharedFolderMetaData>> ListSharedFolders()
        {
            var entries = new List<SharedFolderMetaData>();

            var listSharedFoldersRequest = new ListSharedFoldersRequest();
            var listSharedFoldersResponse = await PerformRequest<ListSharedFoldersRequest, ListSharedFoldersResponse>(listSharedFoldersRequest, "2/sharing/list_folders");

            if (listSharedFoldersResponse.Entries != null)
                entries.AddRange(listSharedFoldersResponse.Entries);

            var hasMore = !string.IsNullOrEmpty(listSharedFoldersResponse.Cursor);
            var cursor = listSharedFoldersResponse.Cursor;
            var counter = 0;

            while (hasMore)
            {
                counter++;
                if (counter > 1000)
                {
                    throw new Exception("List shared folder request overflow!");
                }

                var continueRequest = new ListSharedFoldersContinueRequest(cursor);
                var continueResponse = await PerformRequest<ListSharedFoldersContinueRequest, ListSharedFoldersContinueResponse>(continueRequest, "2/sharing/list_folders/continue");

                if (continueResponse.Entries != null)
                    entries.AddRange(continueResponse.Entries);

                hasMore = !string.IsNullOrEmpty(continueResponse.Cursor);
                cursor = continueResponse.Cursor;
            }

            return entries;
        }

        private async Task<T> PerformRequest<T>(string path)
        {
            if (_token.IsCancellationRequested)
                throw new TaskCanceledException();

            var retries = 0;

            do
            {
                retries++;

                var request = new HttpRequestMessage(HttpMethod.Post, path);
                var httpResponse = await _client.SendAsync(request, _token);
                var responseContent = await httpResponse.Content.ReadAsStringAsync(_token);

                if (string.IsNullOrEmpty(responseContent))
                    continue;

                try
                {
                    var deserializedResponse = JsonSerializer.Deserialize<T>(responseContent);
                    return deserializedResponse;
                }
                catch (JsonException ex)
                {
                    throw new Exception($"JSON: {responseContent}\r\n\r\nFailed to parse JSON data from API", ex);
                }
                catch (Exception)
                {
                    // Unknown Exception
                }
            } while (retries < 5);

            throw new Exception("Request to Dropbox-API failed even after 5 retries...");
        }

        [RequiresUnreferencedCode("Calls System.Text.Json.JsonSerializer.Serialize<TApi>(TApi, System.Text.Json.JsonSerializerOptions?)")]
        private async Task<T> PerformRequest<TApi, T>(TApi apiRequest, string path)
            where TApi : ApiRequest<T>
        {
            if (_token.IsCancellationRequested)
                throw new TaskCanceledException();

            var retries = 0;

            do
            {
                retries++;

                var requestContent = JsonSerializer.Serialize(apiRequest);
                var request = new HttpRequestMessage(HttpMethod.Post, path)
                {
                    Content = new StringContent(requestContent, Encoding.UTF8, "application/json")
                };
                var httpResponse = await _client.SendAsync(request, _token);
                var responseContent = await httpResponse.Content.ReadAsStringAsync(_token);

                if (string.IsNullOrEmpty(responseContent))
                    continue;

                try
                {
                    var deserializedResponse = JsonSerializer.Deserialize<T>(responseContent);
                    return deserializedResponse;
                }
                catch (JsonException ex)
                {
                    throw new Exception($"JSON: {responseContent}\r\n\r\nFailed to parse JSON data from API", ex);
                }
                catch (Exception)
                {
                    // Unknown Exception
                }
            } while (retries < 5);

            throw new Exception("Request to Dropbox-API failed even after 5 retries...");
        }
    }
}