using System.Collections.Generic;

namespace CodacyCSharp.DocsGenerator
{
    public class ParametersIndex
    {
        public string Id { get; set; }
        public List<Parameter> Parameters { get; set; }
    }

    public class Parameter
    {
        public string Key { get; set; }
        public string Description { get; set; }
        public string Type { get; set; }
        public string DefaultValue { get; set; }
    }
}
