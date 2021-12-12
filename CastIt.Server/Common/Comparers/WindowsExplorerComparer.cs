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
            return StrCmpLogicalW(x, y);
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
            return _mode switch
            {
                SortModeType.AlphabeticalPathAsc => StrCmpLogicalW(x!.Path, y!.Path),
                SortModeType.AlphabeticalPathDesc => StrCmpLogicalW(y!.Path, x!.Path),
                SortModeType.AlphabeticalNameAsc => StrCmpLogicalW(x!.Filename, y!.Filename),
                SortModeType.AlphabeticalNameDesc => StrCmpLogicalW(y!.Filename, x!.Filename),
                SortModeType.DurationAsc => x!.TotalSeconds.CompareTo(y!.TotalSeconds),
                SortModeType.DurationDesc => y!.TotalSeconds.CompareTo(x!.TotalSeconds),
                _ => throw new ArgumentOutOfRangeException(nameof(_mode), _mode, "Invalid sort mode")
            };
        }
    }
}
