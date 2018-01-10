using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LibGit2Sharp;

namespace ConsoleApp1
{
    internal class LinesCounter : IDisposable
    {
        private static ConcurrentDictionary<string, int> _calculatedLengths = new ConcurrentDictionary<string, int>();
        private readonly Repository _repo;
        internal static EventHandler<StringEventArgs> Output;

        protected LinesCounter(in string path) => _repo = new Repository(path);

        /// <summary>
        /// Returns the number of lines, per commit, in a list ordered, by date, from last to newest
        /// </summary>
        /// <param name="path">path to the repository folder</param>
        /// <returns></returns>
        internal static List<CommitWithNumber> GetStats(in string path)
        {
            using (var linesCounter = new LinesCounter(path))
            {
                return linesCounter.GetStats();
            }
        }

        /// <summary>
        /// Returns the number of lines, per commit, with additional info, in a CSV format separated by pipe characters '|' with 1st row as column names
        /// </summary>
        /// <param name="path">path to the repository folder</param>
        /// <returns></returns>
        internal static string GetStatsFormatted(in string path)
        {
            using (var linesCounter = new LinesCounter(path))
            {
                return linesCounter.GetStatsFormatted();
            }
        }

        private Patch Compare(Commit newCommit, Commit oldCommit) => _repo.Diff.Compare<Patch>(oldCommit?.Tree, newCommit?.Tree);

        private int ComputeNumberOfLines(Commit commit)
        {
            if (commit == null) { return 0; }
            if (_calculatedLengths.TryGetValue(commit.Sha, out var calculated)) { return calculated; }

            var numberOfLines = 0;

            //Take only left branch (main parent's full number of lines) + the change from the right parent
            var parentCommit1 = commit.Parents.ElementAtOrDefault(0);
            var patch1 = Compare(commit, parentCommit1);
            numberOfLines += Compute(
                patch1,
                parentCommit1);

            //if the left patch contains no changes, it means that the changes from patch2 are already contained in patch1.. I think..
            if (patch1.LinesAdded != 0 || patch1.LinesDeleted != 0)
            {
                //right branch contribution
                var parentCommit2 = commit.Parents.ElementAtOrDefault(1);
                if (parentCommit2 != null)
                {
                    var patch2 = Compare(commit, parentCommit2);
                    numberOfLines += (patch2.LinesAdded - patch2.LinesDeleted);
                }
            }
            int Compute(Patch patch, Commit parentCommit) => (patch.LinesAdded - patch.LinesDeleted) + ComputeNumberOfLines(parentCommit);

            _calculatedLengths.TryAdd(commit.Sha, numberOfLines);
            Output?.Invoke(this, new StringEventArgs($"{commit.Sha} | {numberOfLines}"));
            return numberOfLines;
        }

        private List<CommitWithNumber> GetStats()
        {
            var list = new List<CommitWithNumber>();
            foreach (var commit in _repo.Commits.OrderByDescending(o => o.Author.When))
            {
                list.Add(
                    new CommitWithNumber(
                        commit,
                        ComputeNumberOfLines(commit)));
            }
            return list;
        }

        private string GetStatsFormatted()
        {
            var list = GetStats();

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

        internal class CommitWithNumber
        {
            public CommitWithNumber(Commit commit, int linesCount)
            {
                Commit = commit;
                NumberOfLines = linesCount;
            }

            public Commit Commit { get; }
            public int NumberOfLines { get; }

            public override bool Equals(object obj)
            {
                var number = obj as CommitWithNumber;
                return number != null &&
                       EqualityComparer<Commit>.Default.Equals(Commit, number.Commit) &&
                       NumberOfLines == number.NumberOfLines;
            }

            public override int GetHashCode()
            {
                var hashCode = -887049516;
                hashCode = hashCode * -1521134295 + EqualityComparer<Commit>.Default.GetHashCode(Commit);
                hashCode = hashCode * -1521134295 + NumberOfLines.GetHashCode();
                return hashCode;
            }
        }

        internal class StringEventArgs : EventArgs
        {
            public StringEventArgs(string message) => Message = message;

            public string Message { get; }
        }

        #region IDisposable Support

        private bool disposedValue = false;

        public void Dispose() =>
            Dispose(true);

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    _repo?.Dispose();
                }
                disposedValue = true;
            }
        }

        #endregion IDisposable Support
    }
}