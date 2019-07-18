using System;

namespace CodacyCSharp.Seed
{
    public static class Logger
    {
        private static readonly bool debugFlag = Convert.ToBoolean(Environment.GetEnvironmentVariable("DEBUG"));

        public static void Send<T>(T message)
        {
            if (debugFlag)
            {
                Console.WriteLine(message);
            }
        }
    }
}
