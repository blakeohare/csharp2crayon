using System;
using System.Collections.Generic;

namespace CSharp2Crayon.Parser.Nodes
{

    public class VerifiedFieldReference : Expression
    {
        public Token Name { get; private set; }
        public Expression RootValue { get; private set; }
        public ResolvedType StaticMethodSource { get; private set; }

        public MethodDefinition Method { get; private set; }
        public FieldDefinition Field { get; private set; }
        // TODO: add a FrameworkMethod class

        private MethodRefType type = MethodRefType.UNKNOWN;

        private enum MethodRefType
        {
            UNKNOWN,

            LINQ,
            FIELD,
            STATIC_FIELD,
            METHOD,
            STATIC_METHOD,
            FRAMEWORK_METHOD,
            STATIC_FRAMEWORK_METHOD,
            STRING_METHOD,
        }

        public VerifiedFieldReference(
            Token firstToken,
            TopLevelEntity parent,
            Token methodName,
            Expression root,
            ResolvedType staticMethodSource)
            : base(firstToken, parent)
        {
            this.Name = methodName;
            this.RootValue = root;
            this.StaticMethodSource = staticMethodSource;
            this.type = MethodRefType.UNKNOWN;
        }

        public override Expression ResolveTypes(ParserContext context, VariableScope varScope)
        {
            // This gets created in the ResolveTypes phase.
            throw new InvalidOperationException();
        }

        public void ResolveMethodReference(ParserContext context, ResolvedType[] argTypes)
        {
            if (this.StaticMethodSource != null)
            {
                if (this.StaticMethodSource.CustomType != null)
                {
                    this.ResolveClassStaticMethodReference(argTypes);
                }
                else if (this.StaticMethodSource.FrameworkClass != null)
                {
                    this.ResolveFrameworkStaticMethodReference(argTypes);
                }
                else if (this.StaticMethodSource.PrimitiveType != null)
                {
                    this.ResolvePrimitiveStaticMethodReference(argTypes);
                }
                else
                {
                    throw new ParserException(this.FirstToken, "Not implemented");
                }
            }
            else
            {
                ResolvedType rootType = this.RootValue.ResolvedType;
                if (this.FileContext.HasLinq && rootType.IsEnumerable(context))
                {
                    switch (this.Name.Value)
                    {
                        case "OrderBy": throw new System.NotImplementedException();
                        case "ToArray": throw new System.NotImplementedException();
                        case "Select": throw new System.NotImplementedException();
                        case "Where": throw new System.NotImplementedException();
                        default: break;
                    }
                }

                if (rootType.CustomType != null)
                {
                    this.ResolveCustomMethodReference(argTypes);
                }
                else if (rootType.FrameworkClass != null)
                {
                    this.ResolveFrameworkMethodReference(argTypes);
                }
                else
                {
                    throw new ParserException(this.FirstToken, "Not implemented");
                }
            }
        }

        private void ResolveClassStaticMethodReference(ResolvedType[] argTypes)
        {
            throw new ParserException(this.FirstToken, "Not implemented");
        }

        private void ResolveCustomMethodReference(ResolvedType[] argTypes)
        {
            throw new ParserException(this.FirstToken, "Not implemented");
        }

        private void ResolveFrameworkMethodReference(ResolvedType[] argTypes)
        {
            throw new ParserException(this.FirstToken, "Not implemented");
        }

        private void ResolveFrameworkStaticMethodReference(ResolvedType[] argTypes)
        {
            throw new ParserException(this.FirstToken, "Not implemented");
        }

        private void ResolvePrimitiveStaticMethodReference(ResolvedType[] argTypes)
        {
            throw new ParserException(this.FirstToken, "Not implemented");
        }
    }
}
