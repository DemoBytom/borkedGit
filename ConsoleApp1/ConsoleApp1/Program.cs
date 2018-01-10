using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using LibGit2Sharp;
using static System.Console;

namespace ConsoleApp1
{
    internal static class Program
    {
        private static void Main(string[] args)
        {
            foreach (var path in args
                .Where(o => Repository.IsValid(Path.GetFullPath(o))))
            {
                var sw = Stopwatch.StartNew();
                var repoName = Path.GetFileName(Path.GetFullPath(path));
                var finalFilePath = Path.GetFullPath($"output/{repoName}.txt");

                Directory.CreateDirectory(Path.GetDirectoryName(finalFilePath));
                WriteLine($"Calculating statistics for {repoName} repository.");
                var repo = new Repository(path);

                LinesCounter.Output += (sender, e) => WriteGraph(e.LineNumber, repoName);

                using (var file = File.Create(finalFilePath))
                using (var writer = new StreamWriter(file))
                {
                    writer.Write(LinesCounter.GetStatsFormatted(path));
                }
                sw.Stop();
                WriteLine($"Finished calculating statistics for {repoName} after {sw.Elapsed}. Report written to {finalFilePath}");
            }
        }

        private static List<int> values = new List<int>();
        private static int maxValue = 0;
        private static int oneNth = 0;

        private static void WriteGraph(int linesNumber, string repoName)
        {
            values.Insert(0, linesNumber);
            maxValue = maxValue < linesNumber
                ? linesNumber
                : maxValue;

            var numberOfLines = 25;
            var lineLength = 100;

            var nth = Math.Max(values.Count / numberOfLines, 1);

            oneNth = nth;

            SetCursorPosition(0, 0);
            var sb = new StringBuilder()
                .AppendLine($"Calculating statistics for {repoName} repository.")
                .AppendLine();
            for (var i = 0;
                i < numberOfLines && i < values.Count;
                ++i)
            {
                var nThValues = (int)values
                    .Skip(i * oneNth)
                    .Take(oneNth)
                    .Average();

                var realValue = nThValues * lineLength / maxValue;

                sb
                    .Append("".PadRight(5, ' '))
                    .Append(nThValues.ToString()
                        .PadRight(9))
                    .AppendLine("*"
                        .PadLeft(realValue, '*')
                        .PadRight(lineLength, ' '));
            }

            WriteLine(sb);
        }
    }
}