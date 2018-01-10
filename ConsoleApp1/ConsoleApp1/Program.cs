using System.Diagnostics;
using System.IO;
using System.Linq;
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
                LinesCounter.Output += (sender, e) => WriteLine(e.Message);
                using (var file = File.Create(finalFilePath))
                using (var writer = new StreamWriter(file))
                {
                    writer.Write(LinesCounter.GetStatsFormatted(path));
                }
                sw.Stop();
                WriteLine($"Finished calculating statistics for {repoName} after {sw.Elapsed}. Report written to {finalFilePath}");
            }
        }
    }
}