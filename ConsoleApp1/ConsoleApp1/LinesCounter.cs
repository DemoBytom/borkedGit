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
        private static readonly ConcurrentDictionary<string, (int lineNumber, int added, int deleted)> _calculatedLengths = new ConcurrentDictionary<string, (int, int, int)>();
        private readonly Repository _repo;
        internal static EventHandler<LinesCounterEventArgs> Output;

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

        private (int linesNumber, int added, int deleted) ComputeNumberOfLines(Commit commit)
        {
            if (commit == null) { return (0, 0, 0); }
            if (_calculatedLengths.TryGetValue(commit.Sha, out var calculated)) { return calculated; }

            var numberOfLines = 0;

            //Take only left branch (main parent's full number of lines). Don't code at 3 am or you gonna make mistakes >.>'
            var parentCommit1 = commit.Parents.ElementAtOrDefault(0);
            var patch1 = Compare(commit, parentCommit1);
            numberOfLines += Compute(
                patch1,
                parentCommit1);

            _calculatedLengths.TryAdd(commit.Sha, (numberOfLines, patch1.LinesAdded, patch1.LinesDeleted));
            return (numberOfLines, patch1.LinesAdded, patch1.LinesDeleted);

            int Compute(Patch patch, Commit parentCommit) => (patch.LinesAdded - patch.LinesDeleted) + ComputeNumberOfLines(parentCommit).linesNumber;
        }

        private List<CommitWithNumber> GetStats()
        {
            var list = new List<CommitWithNumber>();
            foreach (var commit in _repo.Commits.OrderByDescending(o => o.Author.When))
            {
                var (linesNumber, added, deleted) = ComputeNumberOfLines(commit);
                Output?.Invoke(this, new LinesCounterEventArgs(commit.Sha, linesNumber));

                list.Add(
                    new CommitWithNumber(
                        commit,
                        linesNumber,
                        added,
                        deleted));
            }
            return list;
        }

        private string GetStatsFormatted()
        {
            var list = GetStats();

            var sb = new StringBuilder();
            sb.AppendLine("SHA|Date|Author|Lines count|Lines added|Lines deleted|Message");

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
                    .Append(list[i2].LinesAdded)
                    .Append("|")
                    .Append(list[i2].LinesDeleted)
                    .Append("|")
                    .AppendLine(list[i2].Commit.MessageShort);
            }

            return sb.ToString();
        }

        internal class CommitWithNumber
        {
            public CommitWithNumber(Commit commit, int linesCount, int linesAdded, int linesDeleted)
            {
                Commit = commit;
                NumberOfLines = linesCount;
                LinesAdded = linesAdded;
                LinesDeleted = linesDeleted;
            }

            public Commit Commit { get; }
            public int NumberOfLines { get; }
            public int LinesAdded { get; }
            public int LinesDeleted { get; }

            public override bool Equals(object obj)
            {
                var number = obj as CommitWithNumber;
                return number != null
                       && EqualityComparer<Commit>.Default.Equals(Commit, number.Commit)
                       && NumberOfLines == number.NumberOfLines;
            }

            public override int GetHashCode()
            {
                var hashCode = -887049516;
                hashCode = (hashCode * -1521134295) + EqualityComparer<Commit>.Default.GetHashCode(Commit);
                return (hashCode * -1521134295) + NumberOfLines.GetHashCode();
            }
        }

        internal class LinesCounterEventArgs : EventArgs
        {
            public LinesCounterEventArgs(string sha, int lineNumber)
            {
                Sha = sha;
                LineNumber = lineNumber;
            }

            public string Message => $"{Sha} | {LineNumber}";
            public string Sha { get; }

            public int LineNumber { get; }
        }

        #region IDisposable Support

        private bool disposedValue = false;

        /// <summary>Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.</summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

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