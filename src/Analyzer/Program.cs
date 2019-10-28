namespace CodacyCSharp.Analyzer
{
    internal static class Program
    {
        private static int Main()
        {
            using (var analyzer = new CodeAnalyzer())
            {
                analyzer.Run()
                    .GetAwaiter().GetResult();
            }

            return 0;
        }
    }
}
