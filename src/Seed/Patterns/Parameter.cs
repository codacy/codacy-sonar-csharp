using Newtonsoft.Json;

namespace CodacyCSharp.Seed.Patterns
{
	public sealed class Parameter
	{
		[JsonProperty (PropertyName = "name")]
		public string Name { get; set; }

		[JsonProperty (PropertyName = "default")]
		public string Default { get; set; }
	}
}
