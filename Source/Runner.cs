using CSharp2Crayon.Parser;
using System.Collections.Generic;
using System.Linq;

namespace CSharp2Crayon
{
    internal class Runner
    {
        private string slnPath;
        private string[] fileBlackList = new string[0];
        private Dictionary<string, bool> buildConstants = new Dictionary<string, bool>();

        public Runner(string solutionPath)
        {
            this.slnPath = solutionPath;
        }

        public Runner SetFileBlackList(ICollection<string> files)
        {
            this.fileBlackList = files.ToArray();
            return this;
        }

        public Runner SetBuildConstant(string name, bool value)
        {
            this.buildConstants[name] = value;
            return this;
        }

        public void Run()
        {
            string slnData = Util.ReadFile(slnPath);
            string slnDir = System.IO.Path.GetDirectoryName(slnPath);

            string[] csharpFiles = Util.GatherFiles(slnDir, ".cs", this.fileBlackList);
            ParserContext parser = new ParserContext();

            foreach (string key in this.buildConstants.Keys)
            {
                parser.SetBuildConstant(key, this.buildConstants[key]);
            }

            foreach (string csharpFile in csharpFiles)
            {
                string content = Util.ReadFile(System.IO.Path.Combine(slnDir, csharpFile));
                parser.ParseFile(csharpFile, content);
            }

            parser.Resolve();
        }
    }
}
