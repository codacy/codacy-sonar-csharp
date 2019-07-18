using System;
using CodacyCSharp.Seed;

namespace CodacyCSharp.Analyzer
{
	static class Program
	{
		static int Main ()
		{
			new CodeAnalyzer ().Run ()
				.GetAwaiter ().GetResult ();

			return 0;
		}
	}
}
