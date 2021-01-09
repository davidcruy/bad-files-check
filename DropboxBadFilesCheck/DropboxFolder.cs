using System.Collections.Generic;
using DropboxBadFilesCheck.Api.Dtos;

namespace DropboxBadFilesCheck
{
    public class DropboxFolder
    {
        public DropboxFolder(string path)
        {
            Path = path;
            IsScanned = false;
            HasInvalidFiles = false;
            Files = new List<FileEntry>();
            InvalidFiles = new List<FileEntry>();

            InvalidFileCount = 0;
        }

        public void AddFile(FileEntry fileEntry)
        {
            Files.Add(fileEntry);
            FileCount++;

            if (fileEntry.IsInvalidFileName())
            {
                InvalidFiles.Add(fileEntry);
                HasInvalidFiles = true;
                InvalidFileCount++;
            }
        }

        public string Path { get; }
        public bool IsScanned { get; set; }
        public bool HasInvalidFiles { get; private set; }

        public int FileCount { get; private set; }
        public int InvalidFileCount { get; private set; }

        public List<FileEntry> Files { get; }
        public List<FileEntry> InvalidFiles { get; }
    }
}