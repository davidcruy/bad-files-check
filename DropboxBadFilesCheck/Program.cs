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

            var cancellationToken = cancellationTokenSource.Token;

#pragma warning disable 4014
            Task.Run(() =>
#pragma warning restore 4014
            {
                Console.WriteLine("Press enter to cancel");
                Console.ReadLine();

                // ReSharper disable once AccessToDisposedClosure
                cancellationTokenSource.Cancel();
            });

            try
            {
                var fileChecker = new DropboxFileChecker(cancellationToken);

                var longRunningTask = Task.Run(async () =>
                {
                    // ReSharper disable once AccessToDisposedClosure
                    await fileChecker.DropboxBadFilesCheck(options.Bearer, options.MaxDepth);
                }, cancellationToken);

                var consoleTask = Task.Run(async () =>
                {
                    var lastCount = 0;
                    Console.Write("Files checked: 0");

                    do
                    {
                        Console.SetCursorPosition(Console.CursorLeft - lastCount.ToString().Length, Console.CursorTop);

                        lastCount = fileChecker.GetEntryCount();
                        Console.Write(lastCount);

                        await Task.Delay(100, cancellationToken);
                    }
                    while (!fileChecker.ScanFinished);

                }, cancellationToken);

                await Task.WhenAll(longRunningTask, consoleTask);
            }
            catch (TaskCanceledException)
            {
                Console.WriteLine("Task was cancelled");
            }
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
