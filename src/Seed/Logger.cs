using System;

namespace CodacyCSharp.Seed
{
	public static class Logger
	{
		private static bool debugFlag = Convert.ToBoolean(Environment.GetEnvironmentVariable("DEBUG"));

		public static void Send<T>(T message)
		{
			if(debugFlag)
			{
				System.Console.WriteLine(message);
			}
		}
	}
}
