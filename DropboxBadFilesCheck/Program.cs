using System;
using System.Threading;
using System.Threading.Tasks;

namespace DropboxBadFilesCheck
{
    class Program
    {
        private class Options
        {
            public string Bearer { get; set; }
            public int MaxDepth { get; set; }
        }

        static async Task Main(string[] args)
        {
            if (!TryParseArgs(args, out var options))
            {
                Console.WriteLine("Usage: dropbox-bfc.exe [-d depth] bearer");
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
                    await fileChecker.DropboxBadFilesCheck(options.Bearer, options.MaxDepth);
                }
                catch (TaskCanceledException) { }
            });

#pragma warning disable 4014
            var consoleTask = Task.Run(async () =>
#pragma warning restore 4014
            {
                Console.CursorVisible = false;
                Console.Write("Folders checked: 0\r\n"); // 17
                Console.Write("Files checked: 0\r\n"); // 15
                Console.Write("\\");

                char[] loader = { '\\', '|', '/', '-', };
                var loaderPos = 0;

                const int leftFolders = 17;
                const int leftFiles = 15;
                const int leftLoader = 0;
                var topFolders = Console.CursorTop - 2;
                var topFiles = Console.CursorTop - 1;
                var topLoader = Console.CursorTop;

                do
                {
                    Console.SetCursorPosition(leftFolders, topFolders);
                    Console.Write(fileChecker.GetFolderCount());

                    Console.SetCursorPosition(leftFiles, topFiles);
                    Console.Write(fileChecker.GetFileCount());

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

                foreach (var fileEntry in fileChecker.GetInvalidFiles())
                {
                    Console.WriteLine(fileEntry);
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

            if (args.Length == 1)
            {
                var bearer = args[0];
                options = new Options
                {
                    Bearer = bearer,
                    MaxDepth = -1
                };

                return true;
            }

            if (args[0] == "-d" && args.Length == 3)
            {
                if (!int.TryParse(args[1], out var depth))
                {
                    options = null;
                    return false;
                }

                var bearer = args[2];
                options = new Options
                {
                    Bearer = bearer,
                    MaxDepth = depth
                };

                return true;
            }

            options = null;
            return false;
        }
    }
}
