using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace CastIt.Common.Comparers
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
}
