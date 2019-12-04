using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace DropboxBadFilesCheck
{
    internal class DropboxFileChecker
    {
        private readonly object _lock = new object();

        private readonly List<DropboxFolder> _folders;
        private readonly ConcurrentQueue<DropboxFolder> _toScan;
        private readonly CancellationToken _token;

        private readonly List<DropboxWorker> _workers;

        public DropboxFileChecker(CancellationToken token)
        {
            _token = token;
            _folders = new List<DropboxFolder>();
            _toScan = new ConcurrentQueue<DropboxFolder>();
            _workers = new List<DropboxWorker>();

            ScanFinished = false;
        }

        public bool ScanFinished { get; private set; }

        public async Task DropboxBadFilesCheck(string bearer, int maxDepth)
        {
            InitializeWorkers(bearer);
            var rootFolder = new DropboxFolder("", 0);

            _toScan.Enqueue(rootFolder);

            while (Working() || _toScan.Count > 0)
            {
                var worker = GetAvailableWorker();

                if (worker != null && _toScan.TryDequeue(out var workingFolder))
                {
                    worker.ScanFolder(workingFolder, maxDepth);
                    continue;
                }

                await Task.Delay(100, _token);
            }

            ScanFinished = true;
        }

        private bool Working() => _workers.Any(w => w.IsWorking);

        private DropboxWorker GetAvailableWorker()
        {
            var worker = _workers.FirstOrDefault(w => w.IsWorking == false);
            return worker;
        }

        private void InitializeWorkers(string bearer)
        {
            for (var i = 0; i < 32; i++)
            {
                var worker = new DropboxWorker(bearer, _token);
                worker.OnScanFinished += (folder, foundFolders) =>
                {
                    lock (_lock)
                    {
                        _folders.Add(folder);
                    }

                    foreach (var subFolder in foundFolders)
                    {
                        _toScan.Enqueue(subFolder);
                    }
                };

                _workers.Add(worker);
            }
        }

        public int GetFolderCount()
        {
            lock (_lock)
            {
                return _folders.Count;
            }
        }

        public int GetFileCount()
        {
            lock (_lock)
            {
                return _folders.Sum(f => f.FileCount);
            }
        }

        public int GetInvalidFileCount()
        {
            lock (_lock)
            {
                return _folders.Sum(f => f.InvalidFileCount);
            }
        }

        public IEnumerable<string> GetInvalidFiles()
        {
            lock (_lock)
            {
                return _folders.SelectMany(f => f.InvalidFiles);
            }
        }
    }
}