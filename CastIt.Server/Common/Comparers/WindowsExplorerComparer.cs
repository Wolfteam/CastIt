using CastIt.Domain.Enums;
using CastIt.Shared.Models;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace CastIt.Server.Common.Comparers
{
    public class WindowsExplorerComparer : IComparer<string>
    {
        [DllImport("shlwapi.dll", CharSet = CharSet.Unicode, ExactSpelling = true)]
        private static extern int StrCmpLogicalW(String x, String y);

        public int Compare(string x, string y)
        {
            if (OperatingSystem.IsWindows())
            {
                return StrCmpLogicalW(x, y);
            }

            return string.Compare(x, y, StringComparison.Ordinal);
        }
    }

    public class WindowsExplorerComparerForServerFileItem : IComparer<ServerFileItem>
    {
        private readonly SortModeType _mode;

        [DllImport("shlwapi.dll", CharSet = CharSet.Unicode, ExactSpelling = true)]
        private static extern int StrCmpLogicalW(String x, String y);

        public WindowsExplorerComparerForServerFileItem(SortModeType mode)
        {
            _mode = mode;
        }

        public int Compare(ServerFileItem x, ServerFileItem y)
        {
            if (x == null || y == null)
            {
                throw new ArgumentNullException();
            }

            bool isWindows = OperatingSystem.IsWindows();
            return _mode switch
            {
                SortModeType.AlphabeticalPathAsc => isWindows
                    ? StrCmpLogicalW(x.Path, y.Path)
                    : string.Compare(x.Path, y.Path, StringComparison.Ordinal),
                SortModeType.AlphabeticalPathDesc => isWindows
                    ? StrCmpLogicalW(y.Path, x.Path)
                    : string.Compare(y.Path, x.Path, StringComparison.Ordinal),
                SortModeType.AlphabeticalNameAsc => isWindows
                    ? StrCmpLogicalW(x.Filename, y.Filename)
                    : string.Compare(x.Filename, y.Filename, StringComparison.Ordinal),
                SortModeType.AlphabeticalNameDesc => isWindows
                    ? StrCmpLogicalW(y.Filename, x.Filename)
                    : string.Compare(y.Filename, x.Filename, StringComparison.Ordinal),
                SortModeType.DurationAsc => x.TotalSeconds.CompareTo(y.TotalSeconds),
                SortModeType.DurationDesc => y.TotalSeconds.CompareTo(x.TotalSeconds),
                _ => throw new ArgumentOutOfRangeException(nameof(_mode), _mode, "Invalid sort mode")
            };
        }
    }
}