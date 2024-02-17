using System.Collections.Generic;

namespace CodacyCSharp.DocsGenerator
{
    public class Rule
    {
        public string title { get; set; }
        public string type { get; set; }
        public string status { get; set; }
        public Remediation remediation { get; set; }
        public List<string> tags { get; set; }
        public string defaultSeverity { get; set; }
        public string ruleSpecification { get; set; }
        public string sqKey { get; set; }
        public string scope { get; set; }
        public string quickfix { get; set; }
        public Code code { get; set; }
    }

    public class Remediation
    {
        public string func { get; set; }
        public string constantCost { get; set; }
    }

    public class Code
    {
        public Dictionary<string, string> impacts { get; set; }
        public string attribute { get; set; }
    }
}
