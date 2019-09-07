using System.Collections.Generic;
using System.Linq;

namespace CSharp2Crayon.Parser.Nodes
{
    public abstract class ClassLikeDefinition : TopLevelEntity
    {
        public Token ClassToken { get; private set; }
        public Token Name { get; private set; }
        public CSharpType[] RawSubClassInfoTokens { get; private set; }

        private List<TopLevelEntity> membersBuilder = new List<TopLevelEntity>();
        private TopLevelEntity[] members = null;

        public ClassLikeDefinition(
            Token firstToken,
            Dictionary<string, Token> modifiers,
            Token classToken,
            Token classNameToken,
            List<CSharpType> subClassesAndSuch)
            : base(firstToken)
        {
            this.ClassToken = classToken;
            this.Name = classNameToken;
            this.ApplyModifiers(modifiers);
            this.RawSubClassInfoTokens = subClassesAndSuch.ToArray();
        }

        public void AddMember(TopLevelEntity tle)
        {
            // TODO: check types
            this.members = null;
            this.membersBuilder.Add(tle);
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
    }

    public class ClassDefinition : ClassLikeDefinition
    {
        public ClassDefinition(
            Token firstToken,
            Dictionary<string, Token> modifiers,
            Token classToken,
            Token classNameToken,
            List<CSharpType> subClassesAndSuch)
            : base(firstToken, modifiers, classToken, classNameToken, subClassesAndSuch)
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
            List<CSharpType> subClassesAndSuch)
            : base(firstToken, modifiers, classToken, classNameToken, subClassesAndSuch)
        { }
    }
}
