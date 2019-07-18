using System;

namespace CodacyCSharp.Seed
{
	public static class TimeSpanHelper
	{
		public static TimeSpan Parse (string value)
		{
			string[] timeSplitted = value.Replace ('.', ' ').Split (' ');
			int timeFactor;

			if (timeSplitted.Length > 1)
			{
				switch (timeSplitted[1])
				{
					case "second":
					case "seconds":
						timeFactor = 1;
						break;

					case "minute":
					case "minutes":
						timeFactor = 60;
						break;

					case "hour":
					case "hours":
						timeFactor = 3600;
						break;

					default:
						throw new FormatException ();
				}
			} else
			{
				timeFactor = 1;
			}

			return TimeSpan.FromSeconds (Int32.Parse (timeSplitted[0]) * timeFactor);
		}
	}
}
