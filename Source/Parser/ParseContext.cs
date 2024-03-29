﻿using CSharp2Crayon.Parser.Nodes;
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
                TopLevelEntity tle = TopLevelParser.Parse(this, tokens, null);
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

        public TopLevelEntity DoLookup(string name)
        {
            TopLevelEntity tle;
            return this.entityMap.TryGetValue(name, out tle) ? tle : null;
        }

        public void Resolve()
        {
            this.BuildEntityMap();
            this.ResolveSubclasses();
            this.ResolveTypes();
        }

        private void ResolveSubclasses()
        {
            foreach (TopLevelEntity entity in this.entityMap.Values)
            {
                if (entity is ClassLikeDefinition)
                {
                    ((ClassLikeDefinition)entity).ResolveParentClasses(this);
                }
            }
        }

        private void ResolveTypes()
        {
            TopLevelEntity[] entities = this.entityMap.Keys
                .OrderBy(k => k)
                .Select(k => this.entityMap[k])
                .ToArray();
            foreach (TopLevelEntity tle in entities)
            {
                this.ActiveFileContext = tle.FileContext;
                tle.ResolveTypesForSignatures(this);
            }

            foreach (TopLevelEntity tle in entities)
            {
                this.ActiveFileContext = tle.FileContext;
                tle.ResolveTypesInCode(this);
            }
            this.ActiveFileContext = null;
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
