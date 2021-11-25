using System.Collections.Generic;
using DropboxBadFilesCheck.Api.Dtos;

namespace DropboxBadFilesCheck
{
    public class DropboxFolder
    {
        public DropboxFolder(string path)
        {
            Path = path;
            HasInvalidFiles = false;
            Files = new List<FileEntry>();
            Folders = new List<FileEntry>();
            InvalidFiles = new List<FileEntry>();
            InvalidFolders = new List<FileEntry>();

            InvalidFileCount = 0;
            InvalidFolderCount = 0;
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

        public void AddFolder(FileEntry fileEntry)
        {
            Folders.Add(fileEntry);
            FolderCount++;

            if (fileEntry.IsInvalidFileName())
            {
                InvalidFolders.Add(fileEntry);
                HasInvalidFiles = true;
                InvalidFolderCount++;
            }
        }

        public string Path { get; }
        public bool HasInvalidFiles { get; private set; }

        public int FileCount { get; private set; }
        public int InvalidFileCount { get; private set; }

        public int FolderCount { get; private set; }
        public int InvalidFolderCount { get; private set; }

        public List<FileEntry> Files { get; }
        public List<FileEntry> InvalidFiles { get; }

        public List<FileEntry> Folders { get; }
        public List<FileEntry> InvalidFolders { get; }
    }
}