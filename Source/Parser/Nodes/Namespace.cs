using System.Collections.Generic;
using System.Linq;

namespace CSharp2Crayon.Parser.Nodes
{
    public class Namespace : TopLevelEntity
    {
        public Token[] NameParts { get; private set; }
        public List<TopLevelEntity> Members { get; private set; }

        public override string NameValue
        {
            get { return string.Join(".", this.NameParts.Select(t => t.Value)); }
        }

        public Namespace(Token firstToken, IList<Token> nameParts, TopLevelEntity parent)
            : base(firstToken, parent)
        {
            this.NameParts = nameParts.ToArray();
            this.Members = new List<TopLevelEntity>();
        }

        public void AddMember(TopLevelEntity entity)
        {
            this.Members.Add(entity);
        }
    }
}
