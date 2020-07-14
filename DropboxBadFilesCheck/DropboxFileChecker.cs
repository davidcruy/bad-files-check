using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DropboxBadFilesCheck.Api;

namespace DropboxBadFilesCheck
{
    internal class DropboxFileChecker
    {
        private readonly object _lock = new object();

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

        public async Task DropboxBadFilesCheck(string bearer)
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

            PerformScan();

            ScanFinished = true;
        }

        private void PerformScan()
        {
            foreach (var fileEntry in _api.ListFolder(""))
            {
                if (fileEntry.Tag == "file")
                {
                    lock (_lock)
                    {
                        _root.AddFile(fileEntry);
                    }
                }

                _token.ThrowIfCancellationRequested();
            }
        }

        public int GetFileCount()
        {
            lock (_lock)
            {
                return _root.FileCount;
            }
        }

        public int GetInvalidFileCount()
        {
            lock (_lock)
            {
                return _root.InvalidFileCount;
            }
        }

        public IEnumerable<string> GetInvalidFiles()
        {
            lock (_lock)
            {
                return _root.InvalidFiles.Select(i => i.PathLower + ": " + i.Name);
            }
        }
    }
}