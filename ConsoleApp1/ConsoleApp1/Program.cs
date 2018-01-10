using System;
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

        internal static int ComputeNumberOfLines(this Repository repo, Commit commit)
        {
            if (commit == null) { return 0; }

            var numberOfLines = 0;
            if (commit.Parents.Any())
            {
                foreach (var parentCommit in commit.Parents)
                {
                    var patch = repo.Compare(commit, parentCommit);
                    if (patch.LinesAdded != 0 || patch.LinesDeleted != 0)
                        numberOfLines += ComputeNumberOfLines(
                            patch,
                            parentCommit);
                }
            }
            else
            {
                numberOfLines += ComputeNumberOfLines(
                    repo.Compare(commit, null),
                    null);
            }

            int ComputeNumberOfLines(Patch patch, Commit parentCommit) => (patch.LinesAdded - patch.LinesDeleted) + repo.ComputeNumberOfLines(parentCommit);

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
                //var path = "E:/git/borkedGit/";
                var path = "E:/git/D3DRnD/TestApp1";
                var repo = new Repository(path);
                var info = repo.Info;
                var startCommit = repo.Commits.FirstOrDefault(o => o.Sha.Equals("9e495a116e77f9a32f4f77a03d8b3713d8aa3cca")) as Commit;

                //var A = repo.Lookup("c3694d28c2c640b481e6e40f8b05b0a474225aec") as Commit;
                //var B = repo.Lookup("dc17b646e6b789c83529a570be1e6f5fc636a0ad") as Commit;
                //var C = repo.Lookup("9cad4817e6e1c1ded282739e04a4724e4114b31c") as Commit;
                //var D = repo.Lookup("f22017cd3db918dc112881c53c35df4f2ce37ac2") as Commit;
                //var E = repo.Lookup("9e495a116e77f9a32f4f77a03d8b3713d8aa3cca") as Commit;

                //var patches = repo.Diff.Compare<Patch>(A.Tree, B.Tree);

                //var patches2 = repo.Diff.Compare<Patch>(A.Parents.FirstOrDefault()?.Tree, A.Tree);

                //var i = 0;

                //var patchEA = Compare(E, A);
                //var patchED = Compare(E, D);
                //var patchDC = Compare(D, C);
                //var patchDB = Compare(D, B);
                //var patchCA = Compare(C, A);
                //var patchBA = Compare(B, A);
                //var patchA_ = Compare(A, null);

                //var patchAB = Compare(A, B);

                var A = repo.Lookup("bda6e30c17c830f56e6b188b983b14d15cc8c4f2") as Commit;
                var B = repo.Lookup("95294c53e8b81ba6d6d13f03ba514cf94976d86c") as Commit;
                var C = repo.Lookup("6a63df1c88c695494764df3affc63150476e71f0") as Commit;

                var patchA = repo.Compare(A, B);
                var patcha = Compare(A, B);
                var patchB = repo.Compare(A, C);

                var list = new List<CommitWithNumber>();
                foreach (var commit in repo.Commits.OrderByDescending(o => o.Author.When))
                {
                    list.Add(
                        new CommitWithNumber(
                            commit,
                            repo.ComputeNumberOfLines(commit)));
                }

                //var E_Lines = repo.ComputeNumberOfLines(E);
                //var D_Lines = repo.ComputeNumberOfLines(D);
                //var C_Lines = repo.ComputeNumberOfLines(C);
                //var B_Lines = repo.ComputeNumberOfLines(B);
                //var A_Lines = repo.ComputeNumberOfLines(A);

                var sb = new StringBuilder();
                sb.AppendLine("SHA|Date|Author|Lines count|Message");

                for (
                    int i2 = list.Count - 1;
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
                        .Append(list[i2].Commit.MessageShort)
                        .AppendLine();
                }

                using (var file = File.Create("log.txt"))
                using (var writer = new StreamWriter(file))
                {
                    writer.Write(sb.ToString());
                }

                //foreach (var commit in repo.Commits)
                //{
                //    var lineNumber = repo.ComputeNumberOfLines(commit);
                //    dict[commit.Sha].NumberOfLines = lineNumber;
                //}

                //Compute(startCommit);

                //void Compute(Commit startingCommit)
                //{
                //    foreach (var parentCommit in startingCommit.Parents)
                //    {
                //        var patch = repo.Diff.Compare<Patch>(parentCommit.Tree, startCommit.Tree);
                //        i += patch.LinesAdded;
                //        i -= patch.LinesDeleted;

                //        if (parentCommit != null)
                //            Compute(parentCommit);
                //    }
                //}
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