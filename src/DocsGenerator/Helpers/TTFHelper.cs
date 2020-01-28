using System;

namespace CodacyCSharp.DocsGenerator.Helpers
{
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

            if(sonarTimeToFix == "")
            {
                return 5;
            }

            throw new System.FormatException("Invalid time! '" + sonarTimeToFix + "'");
        }
    }
}
