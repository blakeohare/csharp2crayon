using CSharp2Crayon.Parser.Nodes;
using System.Collections.Generic;
using System.Linq;

namespace CSharp2Crayon.Parser
{
    public class ParserContext
    {
        public FileContext ActiveFileContext { get; private set; }
        private List<TopLevelEntity> topLevelEntities = new List<TopLevelEntity>();
        private Dictionary<string, bool> buildConstants = new Dictionary<string, bool>();
        private Dictionary<string, TopLevelEntity> entityMap = new Dictionary<string, TopLevelEntity>();

        public ParserContext SetBuildConstant(string name, bool value)
        {
            this.buildConstants[name] = value;
            return this;
        }

        public void ParseFile(string fileName, string code)
        {
            this.ActiveFileContext = new FileContext(fileName);
            TokenStream tokens = new TokenStream(
                    fileName,
                    code,
                    buildConstants);

            while (tokens.HasMore)
            {
                TopLevelEntity tle = TopLevelParser.Parse(this, tokens);
                if (tle is UsingDirective)
                {
                    this.ActiveFileContext.AddUsing((UsingDirective)tle);
                }
                else
                {
                    this.topLevelEntities.Add(tle);
                }
            }
            this.ActiveFileContext = null;
        }

        public void Resolve()
        {
            this.BuildEntityMap();
        }

        private void BuildEntityMap()
        {
            foreach (TopLevelEntity entity in this.topLevelEntities)
            {
                Namespace ns = entity as Namespace;
                if (ns != null)
                {
                    string prefix = string.Join('.', ns.NameParts.Select(token => token.Value)) + ".";
                    foreach (TopLevelEntity tle in ns.Members)
                    {
                        this.BuildEntityMapImpl(tle, prefix);
                    }
                }
                else
                {
                    throw new System.NotImplementedException();
                }
            }
        }

        private void BuildEntityMapImpl(TopLevelEntity tle, string prefix)
        {
            if (tle is ClassLikeDefinition)
            {
                string name = ((ClassLikeDefinition)tle).Name.Value;
                this.entityMap[prefix + name] = tle;

                ClassDefinition cd = tle as ClassDefinition;
                if (cd != null)
                {
                    string nestedPrefix = prefix + name + ".";
                    foreach (ClassLikeDefinition nestedClass in cd.NestedClasses)
                    {
                        this.BuildEntityMapImpl(nestedClass, nestedPrefix);
                    }

                    foreach (EnumDefinition enumDef in cd.NestedEnums)
                    {
                        this.BuildEntityMapImpl(enumDef, nestedPrefix);
                    }
                }
            }
            else if (tle is EnumDefinition)
            {
                string name = ((EnumDefinition)tle).Name.Value;
                this.entityMap[prefix + name] = tle;
            }
            else
            {
                throw new System.NotImplementedException();
            }
        }
    }
}
