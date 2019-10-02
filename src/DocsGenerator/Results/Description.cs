using Newtonsoft.Json;

namespace CodacyCSharp.DocsGenerator.Results
{
    public class Description
    {
        [JsonProperty(PropertyName = "patternId")]
        public string PatternId { get; set; }

        [JsonProperty(PropertyName = "title")]
        public string Title { get; set; }

        [JsonProperty(PropertyName = "parameters")]
        public DescriptionParameter[] Parameters { get; set; }
    }
}
