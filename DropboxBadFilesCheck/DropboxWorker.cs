using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DropboxBadFilesCheck.Api;

namespace DropboxBadFilesCheck
{
    public delegate void ScanFinishedDelegate(DropboxFolder folder, IList<DropboxFolder> foundFolders);

    public class DropboxWorker
    {
        private readonly DropboxApi _api;

        public bool IsWorking { get; private set; }

        public DropboxWorker(string bearer, CancellationToken token)
        {
            _api = new DropboxApi(bearer, token);
        }

        public ScanFinishedDelegate OnScanFinished;

        public async void ScanFolder(DropboxFolder folder, int maxDepth)
        {
            IsWorking = true;

            var subFolders = await PerformScan(folder, maxDepth);

            folder.IsScanned = true;
            OnScanFinished(folder, subFolders);

            IsWorking = false;
        }

        private async Task<IList<DropboxFolder>> PerformScan(DropboxFolder folder, int maxDepth)
        {
            if (maxDepth != -1 && folder.Depth >= maxDepth)
            {
                return new List<DropboxFolder>();
            }

            var children = await _api.ListFolder(folder.Path);

            var files = children.Where(e => e.Tag == "file").Select(e => e.Name).ToList();
            var folders = children.Where(e => e.Tag == "folder");

            folder.AddFiles(files);

            var subFolders = new List<DropboxFolder>();

            foreach (var subFolder in folders)
            {
                var sub = new DropboxFolder(subFolder.PathLower, folder.Depth + 1);
                subFolders.Add(sub);
            }

            return subFolders;
        }
    }
}