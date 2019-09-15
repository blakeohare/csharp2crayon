namespace CSharp2Crayon.Parser.Nodes
{
    public class Variable : Expression
    {
        public Token Name { get; private set; }

        public Variable(Token name, TopLevelEntity parent)
            : base(name, parent)
        {
            this.Name = name;
        }

        public override Expression ResolveTypes(ParserContext context, VariableScope varScope)
        {
            string name = this.Name.Value;
            if (name == "this" || name == "base")
            {
                if (name == "this")
                {
                    ClassLikeDefinition cd = this.Parent.ClassContainer;

                    return new ThisKeyword(this.FirstToken, this.Parent)
                    {
                        ResolvedType = ResolvedType.FromClass(cd),
                    };
                }

                // TODO: base
                throw new System.NotImplementedException();
            }

            this.ResolvedType = varScope.GetVariableType(name);
            if (this.ResolvedType == null)
            {
                foreach (TopLevelEntity member in this.ClassContainer.GetMemberNonNull(name))
                {
                    if (member is PropertyDefinition || member is FieldDefinition || member is MethodDefinition)
                    {
                        VerifiedFieldReference vfr;
                        if (member.IsStatic)
                        {
                            ClassLikeDefinition cd = member.ClassContainer;
                            StaticClassReference classRef = new StaticClassReference(this.FirstToken, this.parent, member.ClassContainer);
                            vfr = new VerifiedFieldReference(this.FirstToken, this.parent, this.Name, classRef, ResolvedType.FromClass(cd));
                        }
                        else
                        {
                            ThisKeyword thisKeyword = new ThisKeyword(this.FirstToken, this.Parent);
                            thisKeyword.ResolvedType = ResolvedType.FromClass(this.Parent.ClassContainer);
                            vfr = new VerifiedFieldReference(this.FirstToken, this.parent, this.Name, thisKeyword, null);
                        }

                        if (member is PropertyDefinition)
                        {
                            vfr.Property = (PropertyDefinition)member;
                            vfr.ResolvedType = vfr.Property.ResolvedType;
                        }
                        else if (member is FieldDefinition)
                        {
                            vfr.Field = (FieldDefinition)member;
                            vfr.ResolvedType = vfr.Field.ResolvedType;
                        }
                        else if (member is MethodDefinition)
                        {
                            vfr.Method = (MethodDefinition)member;
                            vfr.ResolvedType = vfr.Method.ResolvedReturnType;
                        }
                        else
                        {
                            throw new System.NotImplementedException();
                        }

                        return vfr;
                    }
                    else
                    {
                        throw new System.NotImplementedException();
                    }
                }

                throw new ParserException(this.FirstToken, "This variable is not declared.");
            }
            return this;
        }
    }
}
