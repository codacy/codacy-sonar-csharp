using Newtonsoft.Json;

namespace CodacyCSharp.Seed.Configuration
{
	public sealed class CodacyConfiguration
	{
		[JsonProperty (PropertyName = "files")]
		public string[] Files { get; set; }

		[JsonProperty (PropertyName = "tools")]
		public Tool[] Tools { get; set; }
	}
}
