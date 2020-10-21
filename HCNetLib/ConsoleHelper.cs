using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace HCNetLib
{
    public static class ConsoleHelper
    {
        static ConsoleHelper()
        {
            var cts = new CancellationTokenSource();
            var task = Task.Factory.StartNew(() =>
            {
                while (!cts.IsCancellationRequested)
                {
                    while (_queue.Count > 0)
                    {
                        var result = _queue.TryDequeue(out var item);
                        if (!result) continue;

                        foreach (var consoleDescription in item)
                        {
                            Console.ResetColor();
                            if (consoleDescription.ForeColor != null)
                                Console.ForegroundColor = consoleDescription.ForeColor.Value;
                            if (consoleDescription.BackColor != null)
                                Console.BackgroundColor = consoleDescription.BackColor.Value;
                            Console.Write(consoleDescription.Content);
                        }

                        Console.WriteLine();
                        Console.ResetColor();
                    }

                    Thread.Sleep(3);
                }
            }, TaskCreationOptions.LongRunning);

            AppDomain.CurrentDomain.ProcessExit += (s, e) =>
            {
                cts.Cancel();
                task.Wait();
            };
        }

        private static readonly ConcurrentQueue<ConsoleDescription[]> _queue =
            new ConcurrentQueue<ConsoleDescription[]>();

        public static void WriteInfo(string data, string module)
        {
            WriteLine(new ConsoleDescription($"[{module}]", ConsoleColor.DarkCyan),
                new ConsoleDescription($" [info] {data}"));
        }

        public static void WriteWarn(string data, string module)
        {
            WriteLine(new ConsoleDescription($"[{module}]", ConsoleColor.DarkCyan),
                new ConsoleDescription($" [warn] {data}", ConsoleColor.Yellow));
        }

        public static void WriteError(string data, string module)
        {
            WriteLine(new ConsoleDescription($"[{module}]", ConsoleColor.Magenta),
                new ConsoleDescription($" [error] {data}", ConsoleColor.Red));
        }

        public static void WriteLine(string content)
        {
            _queue.Enqueue(new[] { new ConsoleDescription(content) });
        }

        public static void WriteLine(params ConsoleDescription[] contents)
        {
            _queue.Enqueue(contents);
        }
    }

    public class ConsoleDescription
    {
        public ConsoleDescription(string content,
            ConsoleColor? foreColor = null,
            ConsoleColor? backColor = null)
        {
            Content = content;
            ForeColor = foreColor;
            BackColor = backColor;
        }

        public string Content { get; set; }
        public ConsoleColor? ForeColor { get; set; }
        public ConsoleColor? BackColor { get; set; }
    }
}
