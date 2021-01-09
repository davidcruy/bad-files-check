using System;
using System.Collections.Generic;
using System.IO;
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
                catch (TaskCanceledException) { }
            });

#pragma warning disable 4014
            var consoleTask = Task.Run(async () =>
#pragma warning restore 4014
            {
                Console.CursorVisible = false;
                Console.Write("Files checked: 0\r\n"); // 15
                Console.Write("Files fixed: 0\r\n"); // 13
                Console.Write("\\");

                char[] loader = { '\\', '|', '/', '-', };
                var loaderPos = 0;

                const int leftFiles = 15;
                const int leftFixes = 13;
                const int leftLoader = 0;
                var topFiles = Console.CursorTop - 2;
                var topFixes = Console.CursorTop - 1;
                var topLoader = Console.CursorTop;

                do
                {
                    Console.SetCursorPosition(leftFiles, topFiles);
                    Console.Write(fileChecker.GetFileCount());

                    Console.SetCursorPosition(leftFixes, topFixes);
                    Console.Write(fileChecker.GetFixedCount());

                    Console.SetCursorPosition(leftLoader, topLoader);
                    loaderPos = loaderPos + 1 == loader.Length ? 0 : loaderPos + 1;
                    Console.Write(loader[loaderPos]);

                    await Task.Delay(50);
                } while (!cancellationTokenSource.IsCancellationRequested && !fileChecker.ScanFinished);

                Console.SetCursorPosition(leftLoader, topLoader);
                Console.Write(' ');

                Console.WriteLine("\r\nScan finished...");
                Console.WriteLine($"Total files: {fileChecker.GetFileCount()}");
                Console.WriteLine($"Invalid files: {fileChecker.GetInvalidFileCount()}\r\n");

                var invalidFiles = fileChecker.GetInvalidFiles();
                foreach (var fileEntry in invalidFiles)
                {
                    Console.WriteLine(fileEntry.PathLower);
                }

                if (!string.IsNullOrEmpty(options.OutputFile))
                {
                    WriteToOutputFile(invalidFiles, options.OutputFile);
                }
            });

#pragma warning disable 4014
            Task.Run(() =>
#pragma warning restore 4014
            {
                Console.ReadLine();

                // ReSharper disable once AccessToDisposedClosure
                cancellationTokenSource.Cancel();
            });

            await Task.WhenAll(longRunningTask, consoleTask);
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

        private static void WriteToOutputFile(IEnumerable<FileEntry> files, string outputFileName)
        {
            var builder = new StringBuilder();

            builder.AppendLine("Filename,Path,MovedPath");

            foreach (var file in files)
            {
                var fileName = PrepareForCsv(file.Name);
                var path = PrepareForCsv(file.PathLower);
                var movedPath = PrepareForCsv(file.MovedPath);

                builder.AppendLine(fileName + "," + path + "," + movedPath);
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
