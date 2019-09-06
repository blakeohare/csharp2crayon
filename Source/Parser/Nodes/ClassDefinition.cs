using System.Collections.Generic;

namespace CSharp2Crayon.Parser.Nodes
{
    public abstract class ClassLikeDefinition : TopLevelEntity
    {
        public Token ClassToken { get; private set; }
        public Token Name { get; private set; }
        public CSharpType[] RawSubClassInfoTokens { get; private set; }

        private List<TopLevelEntity> membersBuilder = new List<TopLevelEntity>();

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
            this.membersBuilder.Add(tle);
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
