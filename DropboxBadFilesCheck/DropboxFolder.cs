using System.Collections.Generic;
using System.Text.RegularExpressions;
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
        }

        public void AddFile(FileEntry fileEntry)
        {
            Files.Add(fileEntry);
            FileCount++;

            if (IsInvalidFileName(fileEntry.Name))
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