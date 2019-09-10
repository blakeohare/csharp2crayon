using System.Collections.Generic;
using System.Linq;
using CSharp2Crayon.Parser.Nodes;

namespace CSharp2Crayon.Parser
{
    public class FileContext
    {
        public string FileName { get; private set; }
        public List<UsingDirective> FileUsings { get; private set; }
        public bool HasLinq { get; private set; }

        public FileContext(string filename)
        {
            this.FileName = filename;
            this.FileUsings = new List<UsingDirective>();

        }

        private string[] namespaceSearchPrefixes = null;
        public string[] NamespaceSearchPrefixes
        {
            get
            {
                if (this.namespaceSearchPrefixes == null)
                {
                    List<string> prefixes = new List<string>() { "" };
                    prefixes.AddRange(this.FileUsings.Select(u => string.Join(".", u.Path.Select(tok => tok.Value))));
                    this.namespaceSearchPrefixes = prefixes.ToArray();
                }
                return this.namespaceSearchPrefixes;
            }
        }

        public void AddUsing(UsingDirective u)
        {
            this.FileUsings.Add(u);
            this.namespaceSearchPrefixes = null;
            bool isLinq = string.Join('.', u.Path.Select(token => token.Value)) == "System.Linq";
            this.HasLinq = this.HasLinq || isLinq;
        }
    }
}
