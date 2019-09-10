using System.Collections.Generic;
using System.Linq;

namespace CSharp2Crayon.Parser.Nodes
{
    public abstract class ClassLikeDefinition : TopLevelEntity
    {
        public Token ClassToken { get; private set; }
        public Token Name { get; private set; }
        public CSharpType[] RawParentClassInfoTokens { get; private set; }
        public ResolvedType[] ParentClasses { get; private set; }

        private List<TopLevelEntity> membersBuilder = new List<TopLevelEntity>();
        private TopLevelEntity[] members = null;

        public override string NameValue { get { return this.Name.Value; } }

        public ClassLikeDefinition(
            Token firstToken,
            Dictionary<string, Token> modifiers,
            Token classToken,
            Token classNameToken,
            List<CSharpType> parentClassesAndSuch,
            TopLevelEntity parent)
            : base(firstToken, parent)
        {
            this.ClassToken = classToken;
            this.Name = classNameToken;
            this.ApplyModifiers(modifiers);
            this.RawParentClassInfoTokens = parentClassesAndSuch.ToArray();
        }

        public void AddMember(TopLevelEntity tle)
        {
            // TODO: check types
            this.members = null;
            this.membersBuilder.Add(tle);
        }

        private Dictionary<string, TopLevelEntity[]> membersByNameLocalOnly = null;
        private Dictionary<string, TopLevelEntity[]> membersByNameFlattened = null;
        public TopLevelEntity[] GetMember(string name)
        {
            if (this.membersByNameFlattened == null)
            {
                Dictionary<string, List<TopLevelEntity>> flattenedBuilder = new Dictionary<string, List<TopLevelEntity>>();
                Dictionary<string, List<TopLevelEntity>> localBuilder = new Dictionary<string, List<TopLevelEntity>>();
                ClassDefinition parent = this.ParentClasses.OfType<ClassDefinition>().FirstOrDefault();
                if (parent != null)
                {
                    parent.GetMember(""); // build cache
                    foreach (string memberName in parent.membersByNameFlattened.Keys)
                    {
                        flattenedBuilder[memberName] = new List<TopLevelEntity>(parent.membersByNameFlattened[memberName]);
                    }
                }

                Dictionary<string, List<TopLevelEntity>>[] dicts = new Dictionary<string, List<TopLevelEntity>>[] {
                    flattenedBuilder,
                    localBuilder,
                };

                List<Dictionary<string, TopLevelEntity[]>> dicts2 = new List<Dictionary<string, TopLevelEntity[]>>();

                foreach (Dictionary<string, List<TopLevelEntity>> dict in dicts)
                {
                    foreach (TopLevelEntity member in this.Members)
                    {
                        string memberName = member.NameValue;
                        if (!dict.ContainsKey(memberName))
                        {
                            dict[memberName] = new List<TopLevelEntity>() { member };
                        }
                        else
                        {
                            dict[memberName].Insert(0, member); // will be looping from the front to find matches.
                        }
                    }
                    Dictionary<string, TopLevelEntity[]> outputDict = new Dictionary<string, TopLevelEntity[]>();
                    foreach (string memberName in dict.Keys)
                    {
                        outputDict[memberName] = dict[memberName].ToArray();
                    }
                    dicts2.Add(outputDict);
                }
                this.membersByNameFlattened = dicts2[0];
                this.membersByNameLocalOnly = dicts2[1];
            }
            TopLevelEntity[] tle;
            return this.membersByNameFlattened.TryGetValue(name, out tle) ? tle : null;
        }

        public TopLevelEntity[] Members
        {
            get
            {
                if (this.members == null)
                {
                    this.members = this.membersBuilder.ToArray();
                }
                return this.members;
            }
        }

        public void ResolveParentClasses(ParserContext context)
        {
            this.ParentClasses = this.RawParentClassInfoTokens.Select(t => this.DoTypeLookup(t, context)).ToArray();
        }

        public override void ResolveTypesForSignatures(ParserContext context)
        {
            foreach (TopLevelEntity entity in this.Members)
            {
                entity.ResolveTypesForSignatures(context);
            }
        }

        public override void ResolveTypesInCode(ParserContext context)
        {
            foreach (TopLevelEntity entity in this.Members)
            {
                entity.ResolveTypesInCode(context);
            }
        }

        public void PopulateParentClassLookup(ParserContext context)
        {
            if (this.allParentTypeIdsPopulated) return;
            this.allParentTypeIdsPopulated = true;

            foreach (ResolvedType rtype in this.ParentClasses)
            {
                ClassLikeDefinition cd = rtype.CustomType as ClassLikeDefinition;
                if (cd != null)
                {
                    cd.PopulateParentClassLookup(context);

                    foreach (string id in cd.AllParentTypeIds)
                    {
                        this.AllParentTypeIds.Add(id);
                    }
                }
                else
                {
                    throw new System.NotImplementedException();
                }
            }
        }
    }

    public class ClassDefinition : ClassLikeDefinition
    {
        public ClassDefinition(
            Token firstToken,
            Dictionary<string, Token> modifiers,
            Token classToken,
            Token classNameToken,
            List<CSharpType> subClassesAndSuch,
            TopLevelEntity parent)
            : base(firstToken, modifiers, classToken, classNameToken, subClassesAndSuch, parent)
        { }

        public ClassLikeDefinition[] NestedClasses
        {
            get { return this.Members.OfType<ClassLikeDefinition>().ToArray(); }
        }

        public EnumDefinition[] NestedEnums
        {
            get { return this.Members.OfType<EnumDefinition>().ToArray(); }
        }
    }

    public class InterfaceDefinition : ClassLikeDefinition
    {
        public InterfaceDefinition(
            Token firstToken,
            Dictionary<string, Token> modifiers,
            Token classToken,
            Token classNameToken,
            List<CSharpType> subClassesAndSuch,
            TopLevelEntity parent)
            : base(firstToken, modifiers, classToken, classNameToken, subClassesAndSuch, parent)
        { }
    }
}
