using System.Collections.Generic;
using System.Linq;
using System.Text;
using LibGit2Sharp;

namespace ConsoleApp1
{
    internal static class LinesCounter
    {
        internal static string CalculateNumberOfLines(in string path)
        {
            var repo = new Repository(path);

            var list = new List<CommitWithNumber>();
            foreach (var commit in repo.Commits.OrderByDescending(o => o.Author.When))
            {
                list.Add(
                    new CommitWithNumber(
                        commit,
                        repo.ComputeNumberOfLines(commit)));
            }

            var sb = new StringBuilder();
            sb.AppendLine("SHA|Date|Author|Lines count|Message");

            for (
                var i2 = list.Count - 1;
                i2 >= 0;
                i2--)
            {
                sb
                    .Append(list[i2].Commit.Sha)
                    .Append("|")
                    .Append(list[i2].Commit.Author.When)
                    .Append("|")
                    .Append(list[i2].Commit.Author.Name)
                    .Append("|")
                    .Append(list[i2].NumberOfLines)
                    .Append("|")
                    .AppendLine(list[i2].Commit.MessageShort);
            }

            return sb.ToString();
        }

        private class CommitWithNumber
        {
            public CommitWithNumber(Commit commit, int linesCount)
            {
                Commit = commit;
                NumberOfLines = linesCount;
            }

            public Commit Commit { get; }
            public int NumberOfLines { get; }
        }
    }
}