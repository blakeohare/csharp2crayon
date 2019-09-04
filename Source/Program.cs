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
                args = new string[] { "/Users/blakeohare/Crayon/Compiler/CrayonWindows.sln" };
            }
            string slnPath = args[0];
            string slnData = Util.ReadFile(slnPath);
            string slnDir = System.IO.Path.GetDirectoryName(slnPath);
            string[] csharpFiles = Util.GatherFiles(slnDir, ".cs");
            TokenStream tokens = new TokenStream();
            foreach (string csharpFile in csharpFiles)
            {
                tokens.AddFile(csharpFile, Util.ReadFile(System.IO.Path.Combine(slnDir, csharpFile)));
            }

            ParserContext parser = new ParserContext();
            while (tokens.HasMore)
            {
                TopLevelEntity tle = TopLevelParser.Parse(parser, tokens);
                parser.AddEntity(tle);
            }

        }

    }
}
