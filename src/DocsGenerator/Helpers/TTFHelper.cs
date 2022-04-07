using System;
using System.Diagnostics.CodeAnalysis;

namespace CodacyCSharp.DocsGenerator.Helpers
{
    [SuppressMessage("", "S101")]
    public static class TTFHelper
    {
        public static long ToCodacyTimeToFix(string sonarTimeToFix)
        {
            int idx;

            idx = sonarTimeToFix.IndexOf("min", StringComparison.CurrentCulture);
            if(idx != -1)
            {
                return long.Parse(sonarTimeToFix.Substring(0, idx));
            }

            idx = sonarTimeToFix.IndexOf("h", StringComparison.CurrentCulture);
            if(idx != -1)
            {
                return long.Parse(sonarTimeToFix.Substring(0, idx)) * 60;
            }

            idx = sonarTimeToFix.IndexOf("d", StringComparison.CurrentCulture);
            if(idx != -1)
            {
                return long.Parse(sonarTimeToFix.Substring(0, idx)) * 60 * 8; // Assuming 8 hours workday
            }

            if(sonarTimeToFix == "")
            {
                return 5;
            }

            throw new System.FormatException("Invalid time! '" + sonarTimeToFix + "'");
        }
    }
}
