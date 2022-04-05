using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DropboxBadFilesCheck.Api;
using DropboxBadFilesCheck.Api.Dtos;

namespace DropboxBadFilesCheck
{
    internal class DropboxFileChecker
    {
        private readonly object _lock = new();

        private readonly DropboxFolder _root;
        private readonly CancellationToken _token;

        private DropboxApi _api;

        public DropboxFileChecker(CancellationToken token)
        {
            _token = token;
            _root = new DropboxFolder("");

            ScanFinished = false;
        }

        public bool ScanFinished { get; private set; }

        public async Task DropboxBadFilesCheck(string bearer, bool fixInvalidFiles)
        {
            _api = new DropboxApi(bearer, _token);
            var currentAccount = await _api.GetCurrentAccount();
            var rootNamespaceId = currentAccount?.RootInfo?.RootNamespaceId;

            _api.SetPathRoot(rootNamespaceId);

            if (string.IsNullOrEmpty(rootNamespaceId))
            {
                ScanFinished = true;
                return;
            }

            await PerformScan(fixInvalidFiles);

            ScanFinished = true;
        }

        private async Task PerformScan(bool fixInvalidFiles)
        {
            await foreach (var fileEntry in _api.ListFolder("").WithCancellation(_token))
            {
                if (fileEntry.Tag == "file")
                {
                    lock (_lock)
                    {
                        _root.AddFile(fileEntry);
                    }
                }

                if (fileEntry.Tag == "folder")
                {
                    lock (_lock)
                    {
                        _root.AddFolder(fileEntry);
                    }
                }

                _token.ThrowIfCancellationRequested();
            }

            if (fixInvalidFiles)
            {
                foreach (var invalidFile in GetInvalidFiles())
                {
                    var fromPath = invalidFile.PathDisplay;

                    var path = fromPath[..^invalidFile.Name.Length];
                    var toPath = path + invalidFile.GetValidFileName();
                    var movedPath = await _api.Move(fromPath, toPath);

                    invalidFile.MovedPath = movedPath;
                }

                foreach (var invalidFolder in GetInvalidFolders())
                {
                    var fromPath = invalidFolder.PathDisplay;

                    var path = fromPath[..^invalidFolder.Name.Length];
                    var toPath = path + invalidFolder.GetValidFileName();
                    var movedPath = await _api.Move(fromPath, toPath);

                    invalidFolder.MovedPath = movedPath;
                }
            }
        }

        public int GetFileCount()
        {
            lock (_lock)
            {
                return _root.FileCount;
            }
        }

        public int GetFolderCount()
        {
            lock (_lock)
            {
                return _root.FolderCount;
            }
        }

        public int GetInvalidFileCount()
        {
            lock (_lock)
            {
                return _root.InvalidFileCount;
            }
        }

        public int GetInvalidFolderCount()
        {
            lock (_lock)
            {
                return _root.InvalidFolderCount;
            }
        }

        public List<FileEntry> GetInvalidFiles()
        {
            lock (_lock)
            {
                return _root.InvalidFiles;
            }
        }

        public List<FileEntry> GetInvalidFolders()
        {
            lock (_lock)
            {
                return _root.InvalidFolders;
            }
        }

        public int GetFixedCount()
        {
            lock (_lock)
            {
                return _root.InvalidFiles.Count(f => !string.IsNullOrEmpty(f.MovedPath));
            }
        }

        public int GetFolderFixedCount()
        {
            lock (_lock)
            {
                return _root.InvalidFolders.Count(f => !string.IsNullOrEmpty(f.MovedPath));
            }
        }
    }
}