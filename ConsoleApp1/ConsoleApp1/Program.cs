using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using LibGit2Sharp;

namespace ConsoleApp1
{
    internal static class Extensions
    {
        internal static Patch Compare(this Repository repo, Commit newCommit, Commit oldCommit) => repo.Diff.Compare<Patch>(oldCommit?.Tree, newCommit?.Tree);

        private static ConcurrentDictionary<string, int> _calculatedLengths = new ConcurrentDictionary<string, int>();

        internal static int ComputeNumberOfLines(this Repository repo, Commit commit)
        {
            if (commit == null) { return 0; }
            if (_calculatedLengths.TryGetValue(commit.Sha, out var calculated)) { return calculated; }

            var numberOfLines = 0;

            //Take only left branch (main parent's full number of lines) + the change from the right parent
            var parentCommit1 = commit.Parents.ElementAtOrDefault(0);
            var patch1 = repo.Compare(commit, parentCommit1);
            numberOfLines += ComputeNumberOfLines(
                patch1,
                parentCommit1);

            //if the left patch contains no changes, it means that the changes from patch2 are already contained in patch1.. I think..
            if (patch1.LinesAdded != 0 || patch1.LinesDeleted != 0)
            {
                //right branch contribution
                var parentCommit2 = commit.Parents.ElementAtOrDefault(1);
                if (parentCommit2 != null)
                {
                    var patch2 = repo.Compare(commit, parentCommit2);
                    numberOfLines += (patch2.LinesAdded - patch2.LinesDeleted);
                }
            }
            int ComputeNumberOfLines(Patch patch, Commit parentCommit) => (patch.LinesAdded - patch.LinesDeleted) + repo.ComputeNumberOfLines(parentCommit);

            _calculatedLengths.TryAdd(commit.Sha, numberOfLines);
            return numberOfLines;
        }
    }

    internal class Program
    {
        private static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            var aa = LibGit2Sharp.Repository.IsValid("D:/git/borkedGit/");

            var repo = new Reporer();
            repo.TestMethod();
        }

        public class Reporer
        {
            public void TestMethod()
            {
                //const string path = "D:/git/borkedGit/";
                //const string path = "E:/git/D3DRnD/TestApp1";
                const string path = "D:/git/hive/processing.netstandard/";
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

                using (var file = File.Create("log.txt"))
                using (var writer = new StreamWriter(file))
                {
                    writer.Write(sb.ToString());
                }
                Patch Compare(Commit newC, Commit oldC) => repo.Diff.Compare<Patch>(oldC?.Tree, newC?.Tree);
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
}