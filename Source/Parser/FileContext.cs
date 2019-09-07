using System;
using System.Collections.Generic;
using CSharp2Crayon.Parser.Nodes;

namespace CSharp2Crayon.Parser
{
    public class FileContext
    {
        public string FileName { get; private set; }
        public List<UsingDirective> FileUsings { get; private set; }

        public FileContext(string filename)
        {
            this.FileName = filename;
            this.FileUsings = new List<UsingDirective>();
        }

        public void AddUsing(UsingDirective u)
        {
            this.FileUsings.Add(u);
        }
    }
}
