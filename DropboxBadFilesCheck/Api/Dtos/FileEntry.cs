using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace DropboxBadFilesCheck.Api.Dtos
{
    public class FileEntry
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName(".tag")]
        public string Tag { get; set; }

        [JsonPropertyName("path_lower")]
        public string PathLower { get; set; }

        [JsonPropertyName("path_display")]
        public string PathDisplay { get; set; }

        public string MovedPath { get; set; }

        public bool IsInvalidFileName()
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
            return Regex.IsMatch(Name, @"^.*[""<>:\/\|?*]+.*$");
        }

        public string GetValidFileName()
        {
            var validFilename = Name;
            var charsToRemove = new[] { "<", ">", ":", "\"", "|", "?", "*" };

            foreach (var c in charsToRemove)
            {
                validFilename = validFilename.Replace(c, string.Empty);
            }

            // Remove period at end of file
            if (validFilename.EndsWith('.'))
            {
                validFilename = validFilename.Substring(0, validFilename.Length - 1);
            }

            return validFilename;
        }
    }
}