using CSharp2Crayon.Parser;
using CSharp2Crayon.Parser.Nodes;
using System;

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

            foreach (string csharpFile in csharpFiles)
            {
                TokenStream tokens = new TokenStream();
                tokens.AddFile(csharpFile, Util.ReadFile(System.IO.Path.Combine(slnDir, csharpFile)));

                while (tokens.HasMore)
                {
                    TopLevelEntity tle = TopLevelParser.Parse(parser, tokens);
                    parser.AddEntity(tle);
                }
            }
        }
    }
}
