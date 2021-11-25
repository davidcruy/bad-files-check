using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DropboxBadFilesCheck.Api.Dtos;

namespace DropboxBadFilesCheck
{
    class Program
    {
        private class Options
        {
            public string Bearer { get; set; }
            public string OutputFile { get; set; }
            public bool FixInvalidFiles { get; set; }
        }

        static async Task Main(string[] args)
        {
            if (!TryParseArgs(args, out var options))
            {
                Console.WriteLine("Usage: dropbox-bfc.exe [-o output.csv] [-f] bearer");
                return;
            }

            using var cancellationTokenSource = new CancellationTokenSource();
            var fileChecker = new DropboxFileChecker(cancellationTokenSource.Token);

            Console.WriteLine("Press enter to cancel");

            var longRunningTask = Task.Run(async () =>
            {
                try
                {
                    // ReSharper disable once AccessToDisposedClosure
                    await fileChecker.DropboxBadFilesCheck(options.Bearer, options.FixInvalidFiles);
                }
                catch (TaskCanceledException)
                {
                }
            }, cancellationTokenSource.Token);

#pragma warning disable 4014
            var consoleTask = Task.Run(async () =>
#pragma warning restore 4014
            {
                Console.CursorVisible = false;
                Console.Write("Files checked: 0\r\n"); // 15
                Console.Write("Folders checked: 0\r\n"); // 17
                Console.Write("Invalid files: 0\r\n"); // 15
                Console.Write("Files fixed: 0\r\n"); // 13
                Console.Write("Invalid folders: 0\r\n"); // 17
                Console.Write("Folders fixed: 0\r\n"); // 15
                Console.Write("\\");

                char[] loader = { '\\', '|', '/', '-', };
                var loaderPos = 0;

                var files = new Point(15, Console.CursorTop - 6);
                var folders = new Point(17, Console.CursorTop - 5);
                var invalidFilesPos = new Point(15, Console.CursorTop - 4);
                var filesFixed = new Point(13, Console.CursorTop - 3);
                var invalidFoldersPos = new Point(17, Console.CursorTop - 2);
                var foldersFixed = new Point(15, Console.CursorTop - 1);

                const int leftLoader = 0;
                var topLoader = Console.CursorTop;

                do
                {
                    UpdateConsole(files, fileChecker, folders, invalidFilesPos, filesFixed, invalidFoldersPos, foldersFixed, leftLoader, topLoader, loaderPos, loader);

                    await Task.Delay(50, cancellationTokenSource.Token);
                } while (!cancellationTokenSource.IsCancellationRequested && !fileChecker.ScanFinished);

                // 1 more time
                UpdateConsole(files, fileChecker, folders, invalidFilesPos, filesFixed, invalidFoldersPos, foldersFixed, leftLoader, topLoader, loaderPos, loader);

                Console.SetCursorPosition(leftLoader, topLoader);
                Console.Write(' ');

                Console.WriteLine("\r\nScan finished...");

                var invalidFiles = fileChecker.GetInvalidFiles();
                if (invalidFiles.Any())
                {
                    Console.WriteLine("\r\nInvalid files:");
                    foreach (var fileEntry in invalidFiles) Console.WriteLine(fileEntry.PathLower);
                }
                else
                    Console.WriteLine("\r\nNo invalid files.");

                var invalidFolders = fileChecker.GetInvalidFolders();
                if (invalidFolders.Any())
                {
                    Console.WriteLine("\r\nInvalid folders:");
                    foreach (var fileEntry in invalidFolders) Console.WriteLine(fileEntry.PathLower);
                }
                else
                    Console.WriteLine("\r\nNo invalid folders.");

                if (!string.IsNullOrEmpty(options.OutputFile))
                {
                    WriteToOutputFile(invalidFiles, invalidFolders, options.OutputFile);
                }
            }, cancellationTokenSource.Token);

#pragma warning disable 4014
            Task.Run(() =>
#pragma warning restore 4014
            {
                Console.ReadLine();

                // ReSharper disable once AccessToDisposedClosure
                cancellationTokenSource.Cancel();
            }, cancellationTokenSource.Token);

            await Task.WhenAll(longRunningTask, consoleTask);
        }

        private static void UpdateConsole(Point files, DropboxFileChecker fileChecker, Point folders, Point invalidFilesPos, Point filesFixed, Point invalidFolders, Point foldersFixed, int leftLoader, int topLoader, int loaderPos, char[] loader)
        {
            Console.SetCursorPosition(files.X, files.Y);
            Console.Write(fileChecker.GetFileCount());

            Console.SetCursorPosition(folders.X, folders.Y);
            Console.Write(fileChecker.GetFolderCount());

            Console.SetCursorPosition(invalidFilesPos.X, invalidFilesPos.Y);
            Console.Write(fileChecker.GetInvalidFileCount());

            Console.SetCursorPosition(filesFixed.X, filesFixed.Y);
            Console.Write(fileChecker.GetFixedCount());

            Console.SetCursorPosition(invalidFolders.X, invalidFolders.Y);
            Console.Write(fileChecker.GetInvalidFolderCount());

            Console.SetCursorPosition(foldersFixed.X, foldersFixed.Y);
            Console.Write(fileChecker.GetFolderFixedCount());

            Console.SetCursorPosition(leftLoader, topLoader);
            loaderPos = loaderPos + 1 == loader.Length ? 0 : loaderPos + 1;
            Console.Write(loader[loaderPos]);
        }

        private static bool TryParseArgs(string[] args, out Options options)
        {
            if (args.Length == 0)
            {
                options = null;
                return false;
            }

            options = new Options();

            for (var i = 0; i < args.Length; i++)
            {
                switch (args[i])
                {
                    case "-o":
                        i++;
                        options.OutputFile = args[i];
                        break;
                    case "-f":
                        options.FixInvalidFiles = true;
                        break;
                    default:
                        options.Bearer = args[i];
                        break;
                }
            }

            if (string.IsNullOrEmpty(options.Bearer))
            {
                options = null;
                return false;
            }

            return true;
        }

        private static void WriteToOutputFile(IEnumerable<FileEntry> files, IEnumerable<FileEntry> folders, string outputFileName)
        {
            var builder = new StringBuilder();

            builder.AppendLine("Filename,Foldername,Path,MovedPath");

            foreach (var file in files)
            {
                var fileName = PrepareForCsv(file.Name);
                var path = PrepareForCsv(file.PathLower);
                var movedPath = PrepareForCsv(file.MovedPath);

                builder.AppendLine(fileName + ",," + path + "," + movedPath);
            }

            foreach (var folder in folders)
            {
                var fileName = PrepareForCsv(folder.Name);
                var path = PrepareForCsv(folder.PathLower);
                var movedPath = PrepareForCsv(folder.MovedPath);

                builder.AppendLine("," + fileName + "," + path + "," + movedPath);
            }

            File.WriteAllText(outputFileName, builder.ToString());
        }

        private static string PrepareForCsv(string input)
        {
            if (string.IsNullOrEmpty(input))
                return "";

            var output = input.Replace("\"", "\"\"");
            output = output.Contains(",")
                ? "\"" + output + "\""
                : output;

            return output;
        }
    }
}