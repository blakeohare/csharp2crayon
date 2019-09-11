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
                throw new System.NotImplementedException();
            }

            this.ResolvedType = varScope.GetVariableType(name);
            if (this.ResolvedType == null)
            {
                foreach (TopLevelEntity member in this.ClassContainer.GetMember(name))
                {
                    if (member is PropertyDefinition || member is FieldDefinition || member is MethodDefinition)
                    {
                        if (member.IsStatic)
                        {
                            ClassLikeDefinition cd = member.ClassContainer;
                            StaticClassReference classRef = new StaticClassReference(this.FirstToken, this.parent, member.ClassContainer);
                            VerifiedFieldReference vfr = new VerifiedFieldReference(this.FirstToken, this.parent, this.Name, classRef, ResolvedType.FromClass(cd));
                            if (member is PropertyDefinition)
                            {
                                vfr.Property = (PropertyDefinition)member;
                            }
                            else if (member is FieldDefinition)
                            {
                                vfr.Field = (FieldDefinition)member;
                            }
                            else if (member is MethodDefinition)
                            {
                                vfr.Method = (MethodDefinition)member;
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
