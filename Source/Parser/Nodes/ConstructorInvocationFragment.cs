namespace CSharp2Crayon.Parser.Nodes
{
    // The purpose of this class is to easily constructor multiple versions of the fragment but with different ResolvedTypes
    public class ConstructorInvocationFragmentWrapper : Expression
    {
        public ConstructorInvocationFragment InnerFragment { get; set; }

        public ConstructorInvocationFragmentWrapper(ConstructorInvocationFragment fragment)
            : base(fragment.FirstToken, fragment.Parent)
        {
            this.InnerFragment = fragment;
        }

        public override Expression ResolveTypes(ParserContext context, VariableScope varScope)
        {
            throw new System.NotImplementedException();
        }
    }

    // This class should not survive past the resolver phase
    public class ConstructorInvocationFragment : Expression
    {
        public enum InitialDataFormatType
        {
            NONE,
            PROPERTIES,
            KEY_VALUES,
            ITEM_LIST,
        }

        public CSharpType ClassName { get; private set; }
        public ResolvedType Class { get; private set; }

        public InitialDataFormatType InitialDataFormat { get; internal set; }
        public Token[] InitialDataPropertyNames { get; internal set; }
        public Expression[] InitialDataKeys { get; internal set; }
        public Expression[] InitialDataValues { get; internal set; }

        public ConstructorInvocationFragment(Token newKeywordToken, CSharpType classNameAsAType, TopLevelEntity parent)
            : base(newKeywordToken, parent)
        {
            this.ClassName = classNameAsAType;
        }

        public override Expression ResolveTypes(ParserContext context, VariableScope varScope)
        {
            this.Class = this.DoTypeLookup(this.ClassName, context);
            // ResolvedType is set by the FunctionInvocation resolver
            return this;
        }

        public void ResolveTypesForInitialData(ParserContext context, VariableScope varScope)
        {
            switch (this.InitialDataFormat)
            {
                case InitialDataFormatType.NONE: break;
                case InitialDataFormatType.ITEM_LIST:
                    if (this.Class.Generics.Length != 1)
                    {
                        throw new ParserException(this.FirstToken, "The generics of this constructor does not support an initialization list.");
                    }
                    ResolvedType itemType = this.Class.Generics[0];
                    for (int i = 0; i < this.InitialDataValues.Length; ++i)
                    {
                        Expression item = this.InitialDataValues[i].ResolveTypes(context, varScope);
                        this.InitialDataValues[i] = item;
                        if (!itemType.CanBeAssignedTo(item.ResolvedType, context))
                        {
                            throw new ParserException(item.FirstToken, "Incorrect type. Cannot convert a " + item.ResolvedType + " to a " + itemType);
                        }
                    }
                    break;
                case InitialDataFormatType.KEY_VALUES:
                    for (int i = 0; i < this.InitialDataValues.Length; ++i)
                    {
                        this.InitialDataKeys[i] = this.InitialDataKeys[i].ResolveTypes(context, varScope);
                        this.InitialDataValues[i] = this.InitialDataValues[i].ResolveTypes(context, varScope);
                    }
                    break;
                case InitialDataFormatType.PROPERTIES:
                    for (int i = 0; i < this.InitialDataValues.Length; ++i)
                    {
                        this.InitialDataValues[i] = this.InitialDataValues[i].ResolveTypes(context, varScope);
                    }
                    break;
            }
        }
    }
}
