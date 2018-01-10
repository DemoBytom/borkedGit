using System.Collections.Concurrent;
using System.Linq;
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
}