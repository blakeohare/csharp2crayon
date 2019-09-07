using CSharp2Crayon.Parser.Nodes;
using System.Collections.Generic;

namespace CSharp2Crayon.Parser
{
    public class ParserContext
    {
        private List<TopLevelEntity> topLevelEntities = new List<TopLevelEntity>();
        private Dictionary<string, bool> buildConstants = new Dictionary<string, bool>();

        public ParserContext SetBuildConstant(string name, bool value)
        {
            this.buildConstants[name] = value;
            return this;
        }

        public void ParseFile(string fileName, string code)
        {
            TokenStream tokens = new TokenStream(
                    fileName,
                    code,
                    buildConstants);

            while (tokens.HasMore)
            {
                TopLevelEntity tle = TopLevelParser.Parse(this, tokens);
                this.topLevelEntities.Add(tle);
            }
        }

        public void Resolve()
        {
            this.ResolveTopLevelRelations();
        }

        private void ResolveTopLevelRelations()
        {

        }
    }
}
