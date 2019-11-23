using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using DropboxBadFilesCheck.Api;
using DropboxBadFilesCheck.Api.Dtos;

namespace DropboxBadFilesCheck
{
    internal class DropboxFileChecker
    {
        private readonly List<FileEntry> _entries;
        private readonly CancellationToken _token;

        public DropboxFileChecker(CancellationToken token)
        {
            _token = token;
            _entries = new List<FileEntry>();

            ScanFinished = false;
        }

        public bool ScanFinished { get; private set; }

        public async Task DropboxBadFilesCheck(string bearer, int maxDepth)
        {
            var api = new DropboxApi(bearer, _token);

            await ScanFolder(api, "", maxDepth);

            ScanFinished = true;

            Console.WriteLine("Scan finished...");
            Console.WriteLine($"Total files: {_entries.Count}");

            var invalidFiles = _entries.Where(e => IsInvalidFileName(e.Name)).ToList();
            Console.WriteLine($"Invalid files: {invalidFiles.Count}");

            foreach (var fileEntry in invalidFiles)
            {
                Console.WriteLine(fileEntry.Name);
            }
        }

        public int GetEntryCount() => _entries.Count;

        private async Task ScanFolder(DropboxApi api, string path, int maxDepth, int currentDepth = 0)
        {
            if (maxDepth != -1 && currentDepth >= maxDepth)
                return;

            var children = await api.ListFolder(path);

            if (_token.IsCancellationRequested)
                throw new TaskCanceledException();

            _entries.AddRange(children.Where(e => e.Tag == "file"));

            foreach (var subFolder in children.Where(e => e.Tag == "folder"))
            {
                await ScanFolder(api, subFolder.PathLower, maxDepth, currentDepth + 1);
            }
        }

        private static bool IsInvalidFileName(string name)
        {
            /*
               < (less than)
               > (greater than)
               : (colon)
               " (double quote)
               | (vertical bar or pipe)
               ? (question mark)
               * (asterisk)
               . (period) or a space at the end of a file or folder name
             */
            return Regex.IsMatch(name, @"^.*[""<>:\/\|?*]+.*$");
        }
    }
}