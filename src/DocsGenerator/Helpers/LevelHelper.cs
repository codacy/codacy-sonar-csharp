using Codacy.Engine.Seed.Patterns;

namespace CodacyCSharp.DocsGenerator.Helpers
{
    public static class LevelHelper
    {
        public static Level ToLevel(string severity)
        {
            switch (severity.ToUpper())
            {
                case "MAJOR":
                    return Level.Warning;

                case "CRITICAL":
                case "BLOCKER":
                    return Level.Error;

                case "MINOR":
                default:
                    return Level.Info;
            }
        }
    }
}
