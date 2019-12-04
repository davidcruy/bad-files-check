using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace DropboxBadFilesCheck
{
    public class DropboxFolder
    {
        public DropboxFolder(string path, int depth)
        {
            Path = path;
            Depth = depth;
            IsScanned = false;
            HasInvalidFiles = false;
            Files = new List<string>();
            InvalidFiles = new List<string>();
        }

        public void AddFiles(IList<string> files)
        {
            Files.AddRange(files);
            InvalidFiles.AddRange(files.Where(IsInvalidFileName).ToList());

            HasInvalidFiles = InvalidFiles.Any();

            FileCount = Files.Count;
            InvalidFileCount = InvalidFiles.Count;
        }

        public string Path { get; }
        public int Depth { get; }
        public bool IsScanned { get; set; }
        public bool HasInvalidFiles { get; private set; }

        public int FileCount { get; private set; }
        public int InvalidFileCount { get; private set; }

        public List<string> Files { get; }
        public List<string> InvalidFiles { get; }

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