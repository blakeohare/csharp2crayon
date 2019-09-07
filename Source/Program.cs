using CSharp2Crayon.Parser;
using CSharp2Crayon.Parser.Nodes;
using System;
using System.Collections.Generic;

namespace CSharp2Crayon
{
    class Program
    {
        private static readonly string[] BLACK_LISTED_CRAYON_FILES = new string[] {
            "Properties/AssemblyInfo.cs", // Not significant to output and is atypical format.

            // The following have syntax stuff that I don't typically use and they'll just be
            // implemented by Mutil anyway, so don't bother...
            "Common/JsonParser.cs",
            "Common/Multimap.cs",
            "Common/Pair.cs",
            "Common/Util.cs",
        };

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

            new Runner(slnPath)
                .SetFileBlackList(BLACK_LISTED_CRAYON_FILES)
                .SetBuildConstant("DEBUG", true)
                .SetBuildConstant("WINDOWS", true)
                .Run();
        }
    }
}
