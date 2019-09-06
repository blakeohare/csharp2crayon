using CSharp2Crayon.Parser;
using CSharp2Crayon.Parser.Nodes;
using System;
using System.Collections.Generic;

namespace CSharp2Crayon
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                args = new string[] {
                    Environment.OSVersion.Platform == PlatformID.Unix
                        ? "/Users/blakeohare/Crayon/Compiler/CrayonWindows.sln"
                        : @"C:\Things\Crayon\Compiler\CrayonWindows.sln"
                };
            }
            string slnPath = args[0];
            string slnData = Util.ReadFile(slnPath);
            string slnDir = System.IO.Path.GetDirectoryName(slnPath);
            string[] csharpFiles = Util.GatherFiles(slnDir, ".cs");
            ParserContext parser = new ParserContext();
            Dictionary<string, bool> buildConstants = new Dictionary<string, bool>() {
                { "DEBUG", true },
                { "WINDOWS", true }
            };

            string[] blackListedFiles = new string[] {
                "Properties/AssemblyInfo.cs", // Not significant to output and is atypical format.

                // The following have syntax stuff that I don't typically use and they'll just be
                // implemented by Mutil anyway, so don't bother...
                "Common/JsonParser.cs",
                "Common/Multimap.cs",
                "Common/Pair.cs",
                "Common/Util.cs",
            };

            foreach (string csharpFile in csharpFiles)
            {
                // blarg
                string canonicalPath = csharpFile.Replace('\\', '/');
                bool skipMe = false;
                foreach (string blEntry in blackListedFiles)
                {
                    if (canonicalPath.EndsWith(blEntry))
                    {
                        skipMe = true;
                        break;
                    }
                }
                if (skipMe) continue;

                TokenStream tokens = new TokenStream(
                    csharpFile,
                    Util.ReadFile(System.IO.Path.Combine(slnDir, csharpFile)),
                    buildConstants);

                while (tokens.HasMore)
                {
                    TopLevelEntity tle = TopLevelParser.Parse(parser, tokens);
                    parser.AddEntity(tle);
                }
            }
        }
    }
}
