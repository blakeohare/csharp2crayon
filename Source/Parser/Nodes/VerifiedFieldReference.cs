using System;
using System.Collections.Generic;
using System.Linq;

namespace CSharp2Crayon.Parser.Nodes
{
    public enum MethodRefType
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
        PRIMITIVE_METHOD,
    }

    public class VerifiedFieldReference : Expression
    {
        public Token Name { get; private set; }
        public Expression RootValue { get; private set; }
        public ResolvedType StaticMethodSource { get; private set; }

        public MethodDefinition Method { get; internal set; }
        public FieldDefinition Field { get; internal set; }
        public PropertyDefinition Property { get; internal set; }
        public string PrimitiveMethod { get; private set; } // primitiveType.FieldName
        // TODO: add a FrameworkMethod class

        public MethodRefType Type { get; internal set; }


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
            this.Type = MethodRefType.UNKNOWN;
        }

        public override Expression ResolveTypes(ParserContext context, VariableScope varScope)
        {
            // This gets created in the ResolveTypes phase.
            throw new InvalidOperationException();
        }

        public ResolvedType GetEnumerableItemTypeGuess(ParserContext context)
        {
            ResolvedType rootType = this.RootValue.ResolvedType;
            return rootType.GetEnumerableItemType();
        }

        public void ResolveFieldReference(ParserContext context, ResolvedType[] argTypes, VariableScope varScope)
        {
            this.TryResolveFieldReference(context, argTypes, varScope);
            if (this.ResolvedType == null)
            {
                throw new ParserException(this.Name, "Could not resolve this field.");
            }
        }

        public void TryResolveFieldReference(ParserContext context, ResolvedType[] argTypes, VariableScope varScope)
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
                if (this.RootValue.ResolvedType == null && this.RootValue is VerifiedFieldReference)
                {
                    this.RootValue.ResolveTypes(context, varScope);
                }
                ResolvedType rootType = this.RootValue.ResolvedType;
                if (this.FileContext.HasLinq && rootType.IsEnumerable(context))
                {
                    this.Type = MethodRefType.LINQ;
                    switch (this.Name.Value)
                    {
                        case "OrderBy":
                            {
                                ResolvedType itemType = rootType.GetEnumerableItemType();
                                ResolvedType functionReturnType = ResolvedType.GetPrimitiveType("object"); // int or string or float or something, but whatever.
                                ResolvedType function = ResolvedType.CreateFunctionPointerType(functionReturnType, new ResolvedType[] { itemType });
                                this.ResolvedType = ResolvedType.CreateFunctionPointerType(
                                    ResolvedType.CreateEnumerableType(itemType),
                                    new ResolvedType[] { function });
                            }
                            break;

                        case "ToArray":
                            {
                                ResolvedType itemType = rootType.GetEnumerableItemType();
                                this.ResolvedType = ResolvedType.CreateFunctionPointerType(
                                    ResolvedType.CreateArray(itemType),
                                    new ResolvedType[0]);
                            }
                            break;

                        case "Select": throw new System.NotImplementedException();
                        case "Where": throw new System.NotImplementedException();
                        default:
                            this.Type = MethodRefType.UNKNOWN;
                            break;
                    }
                    if (this.Type == MethodRefType.LINQ)
                    {
                        return;
                    }
                }

                if (rootType.CustomType != null)
                {
                    if (argTypes == null)
                    {
                        this.ResolveCustomFieldReference();
                    }
                    else
                    {
                        this.ResolveCustomMethodReference(argTypes);
                    }
                }
                else if (rootType.FrameworkClass != null)
                {
                    this.ResolveFrameworkMethodReference(argTypes);
                }
                else if (rootType.PrimitiveType != null)
                {
                    this.Type = MethodRefType.PRIMITIVE_METHOD;
                    this.PrimitiveMethod = rootType.PrimitiveType + "." + this.Name.Value;

                    switch (this.PrimitiveMethod)
                    {
                        case "string.ToLowerInvariant":
                            this.ResolvedType = ResolvedType.CreateFunctionPointerType(ResolvedType.String(), new ResolvedType[0]);
                            break;

                        default:
                            throw new ParserException(this.Name, "string does not have a method called '" + this.Name.Value + "'");
                    }
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

        private void ResolveCustomFieldReference()
        {
            ClassLikeDefinition rootType = (ClassLikeDefinition)this.RootValue.ResolvedType.CustomType;
            string fieldName = this.Name.Value;
            TopLevelEntity member = rootType.GetMember(fieldName).FirstOrDefault();
            if (member == null) throw new ParserException(this.Name, "Not implemented or not found."); // could be a framework field.

            if (member is PropertyDefinition)
            {
                this.Property = (PropertyDefinition)member;
                this.ResolvedType = this.Property.ResolvedType;
            }
            else if (member is FieldDefinition)
            {
                this.Field = (FieldDefinition)member;
                this.ResolvedType = this.Field.ResolvedType;
            }
            else
            {
                throw new ParserException(this.FirstToken, "Not implemented");
            }
        }

        private void ResolveCustomMethodReference(ResolvedType[] argTypes)
        {
            throw new ParserException(this.FirstToken, "Not implemented");
        }

        private void ResolveFrameworkMethodReference(ResolvedType[] argTypes)
        {
            string className = this.RootValue.ResolvedType.FrameworkClass;
            string methodName = this.Name.Value;
            this.Type = MethodRefType.FRAMEWORK_METHOD;
            switch (className + ":" + methodName)
            {
                case "System.Collections.Generic.HashSet:Contains":
                    ResolvedType itemType = this.RootValue.ResolvedType.Generics[0];
                    this.ResolvedType = ResolvedType.CreateFunctionPointerType(ResolvedType.Bool(), new ResolvedType[] { itemType });
                    return;

                case "System.Collections.Generic.Dictionary:Keys":
                    ResolvedType keyType = this.RootValue.ResolvedType.Generics[0];
                    this.ResolvedType = ResolvedType.CreateEnumerableType(keyType);
                    return;

                case "System.Collections.Generic.Dictionary:Values":
                    ResolvedType valueType = this.RootValue.ResolvedType.Generics[1];
                    this.ResolvedType = ResolvedType.CreateEnumerableType(valueType);
                    return;

                default:
                    throw new System.NotImplementedException();
            }
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
