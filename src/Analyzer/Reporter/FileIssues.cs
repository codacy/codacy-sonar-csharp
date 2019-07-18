using System.Collections.Generic;

namespace CodacyCSharp.Analyzer.Reporter
{
    public class FileIssues
    {
        public string FilePath { get; set; }
        public List<Issue> Issues { get; set; }

        public class Issue
        {
            public string Id { get; set; }
            public string Message { get; set; }
        }
    }
}
