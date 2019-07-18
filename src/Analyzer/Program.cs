namespace CodacyCSharp.Analyzer
{
    internal static class Program
    {
        private static int Main()
        {
            new CodeAnalyzer().Run()
                .GetAwaiter().GetResult();

            return 0;
        }
    }
}
