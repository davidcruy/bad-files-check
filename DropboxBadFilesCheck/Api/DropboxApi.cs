using System;
using System.Collections.Generic;
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

        public async Task<IList<FileEntry>> ListFolder(string path)
        {
            var entries = new List<FileEntry>();

            var listFolderRequest = new ListFolderRequest(path);
            var listFolderResponse = await PerformRequest<ListFolderRequest, ListFolderResponse>(listFolderRequest, "2/files/list_folder");
            entries.AddRange(listFolderResponse.Entries);

            var hasMore = listFolderResponse.HasMore;
            var cursor = listFolderResponse.Cursor;
            var counter = 0;

            while (hasMore)
            {
                counter++;
                if (counter > 1000)
                {
                    throw new Exception("List folder request overflow!");
                }

                var continueRequest = new ListFolderContinueRequest(cursor);
                var continueResponse = await PerformRequest<ListFolderContinueRequest, ListFolderContinueResponse>(continueRequest, "2/files/list_folder/continue");

                entries.AddRange(continueResponse.Entries);

                hasMore = continueResponse.HasMore;
                cursor = continueResponse.Cursor;
            }

            return entries;
        }

        private async Task<T> PerformRequest<TApi, T>(TApi apiRequest, string path)
            where TApi : ApiRequest<T>
        {
            if (_token.IsCancellationRequested)
                throw new TaskCanceledException();

            var requestContent = JsonSerializer.Serialize(apiRequest);
            var request = new HttpRequestMessage(HttpMethod.Post, path)
            {
                Content = new StringContent(requestContent, Encoding.UTF8, "application/json")
            };

            var retries = 0;

            do
            {
                var httpResponse = await _client.SendAsync(request, _token);
                var responseContent = await httpResponse.Content.ReadAsStringAsync();

                try
                {
                    var listFolderResponse = JsonSerializer.Deserialize<T>(responseContent);
                    return listFolderResponse;
                }
                catch (JsonException)
                {

                }
                
                retries++;
            } while (retries < 5);

            throw new Exception("Request to Dropbox-API failed even after 5 retries...");
        }
    }
}