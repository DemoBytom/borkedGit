using System.IO;
using System.Linq;
using LibGit2Sharp;

namespace ConsoleApp1
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            foreach (var path in args
                .Where(o => Repository.IsValid(o)))
            {
                var repo = new Repository(path);

                using (var file = File.Create($"{Path.GetFileName(path)}.txt"))
                using (var writer = new StreamWriter(file))
                {
                    writer.Write(LinesCounter.CalculateNumberOfLines(path));
                }
            }
        }
    }
}