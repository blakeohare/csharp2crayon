using System;

namespace CSharp2Crayon
{
    class Program
    {
        private static readonly string[] BLACK_LISTED_CRAYON_FILES = new string[] {
            "Properties/AssemblyInfo.cs", // Not significant to output and is atypical format.
            "CommonUtil/",
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
