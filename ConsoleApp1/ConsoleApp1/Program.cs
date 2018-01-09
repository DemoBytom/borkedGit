using LibGit2Sharp;
using System;
using System.Linq;

namespace ConsoleApp1
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            var aa = LibGit2Sharp.Repository.IsValid(@"D:/git/borkedGit/");

            var repo = new Reporer();
            repo.TestMethod();
        }

        public class Reporer
        {
            public void TestMethod()
            {
                Repository repo = new Repository(@"D:/git/borkedGit/");
                var info = repo.Info;
                var startCommit = repo.Commits.FirstOrDefault(o => o.Sha.Equals("9e495a116e77f9a32f4f77a03d8b3713d8aa3cca")) as Commit;

                var A = repo.Lookup("c3694d28c2c640b481e6e40f8b05b0a474225aec") as Commit;
                var B = repo.Lookup("dc17b646e6b789c83529a570be1e6f5fc636a0ad") as Commit;
                var C = repo.Lookup("9cad4817e6e1c1ded282739e04a4724e4114b31c") as Commit;
                var D = repo.Lookup("f22017cd3db918dc112881c53c35df4f2ce37ac2") as Commit;
                var E = repo.Lookup("9e495a116e77f9a32f4f77a03d8b3713d8aa3cca") as Commit;

                var patches = repo.Diff.Compare<Patch>(A.Tree, B.Tree);

                var patches2 = repo.Diff.Compare<Patch>(A.Parents.FirstOrDefault()?.Tree, A.Tree);

                var i = 0;

                var patchEA = Compare(E, A);
                var patchED = Compare(E, D);
                var patchDC = Compare(D, C);
                var patchDB = Compare(D, B);
                var patchCA = Compare(C, A);
                var patchBA = Compare(B, A);
                var patchA_ = Compare(A, null);

                var patchAB = Compare(A, B);

                Patch Compare(Commit newC, Commit oldC) => repo.Diff.Compare<Patch>(oldC?.Tree, newC?.Tree);
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
            }
        }
    }
}