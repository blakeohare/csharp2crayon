using System.Collections.Generic;
using System.Linq;

namespace CSharp2Crayon.Parser.Nodes
{
    public class FunctionInvocation : Expression
    {
        public Expression Root { get; private set; }
        public Token OpenParen { get; private set; }
        public Expression[] Args { get; private set; }
        public Token[] OutTokens { get; private set; }

        public enum InvocationType
        {
            USER_METHOD,
            USER_STATIC_METHOD,
            CONSTRUCTOR,
        }
        public DotField RootAsDotField { get { return (DotField)this.Root; } }
        public ConstructorInvocationFragment RootAsConstructor { get { return (ConstructorInvocationFragment)this.Root; } }

        public FunctionInvocation(
            Token firstToken,
            Expression root,
            Token openParen,
            IList<Expression> args,
            IList<Token> outTokens,
            TopLevelEntity parent)
            : base(firstToken, parent)
        {
            this.Root = root;
            this.OpenParen = openParen;
            this.Args = args.ToArray();
            this.OutTokens = outTokens.ToArray();
        }

        public override Expression ResolveTypes(ParserContext context, VariableScope varScope)
        {
            // There can be multiple signatures of methods. Calculate all of them.
            // Then resolve the args and filter these down to one.
            // If it's ambiguous or there's no possibilities left, then it's an error.
            List<Expression> possibleRoots = new List<Expression>();

            if (this.Root is Variable)
            {
                Variable v = (Variable)this.Root;
                if (varScope.GetVariableType(v.Name.Value) != null)
                {
                    possibleRoots.Add(this.Root.ResolveTypes(context, varScope));
                }
                else
                {
                    TopLevelEntity[] entities = this.ClassContainer.GetMember(v.Name.Value);

                    foreach (TopLevelEntity entity in entities)
                    {
                        Expression fakeDotFieldRoot = null;
                        if (entity.IsStatic)
                        {
                            fakeDotFieldRoot = new StaticClassReference(this.FirstToken, this.parent, entity.ClassContainer);
                        }
                        else
                        {
                            fakeDotFieldRoot = new ThisKeyword(this.FirstToken, this.parent);
                        }
                        VerifiedFieldReference vfr = new VerifiedFieldReference(this.FirstToken, this.parent, v.Name, fakeDotFieldRoot, null);
                        if (entity is FieldDefinition)
                        {
                            vfr.Field = (FieldDefinition)entity;
                            vfr.ResolvedType = vfr.Field.ResolvedType;
                        }
                        else if (entity is MethodDefinition)
                        {
                            vfr.Method = (MethodDefinition)entity;
                            vfr.ResolvedType = ResolvedType.CreateFunction(
                                vfr.Method.ResolvedReturnType,
                                vfr.Method.ResolvedArgTypes);
                        }
                        else if (entity is PropertyDefinition)
                        {
                            vfr.Property = (PropertyDefinition)entity;
                            vfr.ResolvedType = vfr.Property.ResolvedType;
                        }
                        else
                        {
                            throw new System.InvalidOperationException();
                        }
                        possibleRoots.Add(vfr);
                    }
                }
            }
            else if (this.Root is DotField)
            {
                // Since we know this is a situation where there'll be argument information,
                // don't let the DotFielda attempt to choose one. Just get all possibilities here.
                // Resolve the DotField's root for it inline here.
                DotField df = (DotField)this.Root;
                df.Root = df.Root.ResolveTypes(context, varScope);
                ResolvedType rootResolvedType = df.Root.ResolvedType;
                if (df.Root is StaticClassReference || df.Root is StaticFrameworkClassReference)
                {
                    if (df.Root is StaticClassReference)
                    {
                        throw new System.NotImplementedException();
                    }
                    else
                    {
                        string lookup = rootResolvedType.FrameworkClass + "." + df.FieldName.Value;
                        switch (lookup)
                        {
                            // (string) => bool
                            case "CommonUtil.Disk.FileUtil.DirectoryExists":
                            case "CommonUtil.Disk.FileUtil.FileExists":
                                possibleRoots.Add(ConvertDfToVfr(df, ResolvedType.CreateFunction(
                                    ResolvedType.Bool(),
                                    ResolvedType.String())));
                                break;

                            // (string) => string
                            case "CommonUtil.Environment.EnvironmentVariables.Get":
                            case "CommonUtil.Disk.FileUtil.GetParentDirectory":
                            case "CommonUtil.Disk.Path.GetFileName":
                                possibleRoots.Add(ConvertDfToVfr(df, ResolvedType.CreateFunction(
                                    ResolvedType.String(),
                                    ResolvedType.String())));
                                break;

                            // (string) => string[]
                            case "CommonUtil.Disk.FileUtil.DirectoryListDirectoryPaths":
                                possibleRoots.Add(ConvertDfToVfr(df, ResolvedType.CreateFunction(
                                    ResolvedType.CreateArray(ResolvedType.String()),
                                    ResolvedType.String())));
                                break;

                            // (string, string) => string
                            case "CommonUtil.StringUtil.SplitRemoveEmpty":
                            case "CommonUtil.Disk.FileUtil.GetAbsolutePathFromRelativeOrAbsolutePath":
                                possibleRoots.Add(ConvertDfToVfr(df, ResolvedType.CreateFunction(
                                    ResolvedType.String(),
                                    ResolvedType.String())));
                                possibleRoots.Add(ConvertDfToVfr(df, ResolvedType.CreateFunction(
                                    ResolvedType.String(),
                                    new ResolvedType[] { ResolvedType.String(), ResolvedType.String() })));
                                break;

                            // (params string[]) => string
                            case "CommonUtil.Disk.Path.Join":
                            case "CommonUtil.Disk.FileUtil.JoinPath":
                                List<ResolvedType> paramsStrings = new List<ResolvedType>();
                                for (int i = 0; i < this.Args.Length; ++i)
                                {
                                    paramsStrings.Add(ResolvedType.String());
                                }
                                possibleRoots.Add(ConvertDfToVfr(df, ResolvedType.CreateFunction(
                                    ResolvedType.String(),
                                    paramsStrings.ToArray())));
                                break;

                            default:
                                throw new ParserException(this.FirstToken, "Not implemented");
                        }
                    }
                }
                else
                {
                    if (rootResolvedType.CustomType != null && rootResolvedType.CustomType is ClassLikeDefinition)
                    {
                        TopLevelEntity[] members = ((ClassLikeDefinition)rootResolvedType.CustomType).GetMember(df.FieldName.Value);
                        if (members == null)
                        {
                            throw new ParserException(df.FieldName, "The field '" + df.FieldName.Value + "' does not exist.");
                        }

                        foreach (TopLevelEntity entity in members)
                        {
                            VerifiedFieldReference vfr = new VerifiedFieldReference(this.FirstToken, this.parent, df.FieldName, df.Root, null);
                            if (entity is FieldDefinition)
                            {
                                vfr.Field = (FieldDefinition)entity;
                                vfr.ResolvedType = vfr.Field.ResolvedType;
                            }
                            else if (entity is MethodDefinition)
                            {
                                vfr.Method = (MethodDefinition)entity;
                                vfr.ResolvedType = ResolvedType.CreateFunction(
                                    vfr.Method.ResolvedReturnType,
                                    vfr.Method.ResolvedArgTypes);
                            }
                            else if (entity is PropertyDefinition)
                            {
                                vfr.Property = (PropertyDefinition)entity;
                                vfr.ResolvedType = vfr.Property.ResolvedType;
                            }
                            else
                            {
                                throw new System.NotImplementedException();
                            }
                            possibleRoots.Add(vfr);
                        }
                    }
                    else if (rootResolvedType.FrameworkClass != null || rootResolvedType.IsArray)
                    {
                        if (rootResolvedType.IsEnumerable(context))
                        {
                            ResolvedType itemType = rootResolvedType.GetEnumerableItemType();
                            if (this.FileContext.HasLinq)
                            {
                                switch (df.FieldName.Value)
                                {
                                    case "Concat":
                                        possibleRoots.Add(ConvertDfToLinqVfr(df, ResolvedType.CreateFunction(
                                            ResolvedType.CreateEnumerableType(itemType),
                                            ResolvedType.CreateEnumerableType(itemType))));
                                        break;

                                    case "OrderBy":
                                        possibleRoots.Add(ConvertDfToLinqVfr(df, ResolvedType.CreateFunction(
                                            ResolvedType.CreateEnumerableType(itemType),
                                            ResolvedType.CreateFunction(
                                                ResolvedType.Object(),
                                                itemType))));
                                        break;

                                    case "ToArray":
                                        possibleRoots.Add(ConvertDfToLinqVfr(df, ResolvedType.CreateFunction(
                                            ResolvedType.CreateArray(itemType))));
                                        break;

                                    case "ToDictionary":
                                        possibleRoots.Add(ConvertDfToLinqVfr(df, ResolvedType.CreateFunction(
                                            ResolvedType.CreateDictionary(null, itemType),
                                            ResolvedType.CreateFunction(null, itemType))));
                                        break;

                                    case "Select":
                                        CSharpType[] inlineTypes = df.InlineTypeSpecification;
                                        if (inlineTypes == null)
                                        {
                                            // null means retroactively apply the return type of the function
                                            possibleRoots.Add(ConvertDfToLinqVfr(df, ResolvedType.CreateFunction(
                                                ResolvedType.CreateEnumerableType(null),
                                                ResolvedType.CreateFunction(null, itemType))));
                                        }
                                        else
                                        {
                                            if (inlineTypes.Length != 2)
                                            {
                                                throw new ParserException(df.FieldName, "Linq's .Select needs 2 inline types.");
                                            }

                                            ResolvedType[] resolvedInlineTypes = inlineTypes
                                                .Select(it => this.DoTypeLookup(it, context))
                                                .ToArray();

                                            possibleRoots.Add(ConvertDfToLinqVfr(df, ResolvedType.CreateFunction(
                                                ResolvedType.CreateEnumerableType(itemType),
                                                ResolvedType.CreateFunction(resolvedInlineTypes[1], resolvedInlineTypes[0]))));
                                        }
                                        break;

                                    case "Where": throw new System.NotImplementedException();
                                    case "OfType": throw new System.NotImplementedException();
                                    case "Cast": throw new System.NotImplementedException();
                                    default: break;
                                }
                            }
                        }

                        switch (rootResolvedType.FrameworkClass)
                        {
                            case "System.Text.StringBuilder":
                                {
                                    switch (df.FieldName.Value)
                                    {
                                        case "Append":
                                            possibleRoots.Add(ConvertDfToVfr(df,
                                                ResolvedType.CreateFunction(ResolvedType.Void(), ResolvedType.Object())));
                                            break;
                                    }
                                }
                                break;

                            case "System.Collections.Generic.Stack":
                                {
                                    ResolvedType itemType = rootResolvedType.GetEnumerableItemType();
                                    switch (df.FieldName.Value)
                                    {
                                        case "Push":
                                            possibleRoots.Add(ConvertDfToVfr(df,
                                                ResolvedType.CreateFunction(ResolvedType.Void(), itemType)));
                                            break;

                                        case "Pop":
                                            possibleRoots.Add(ConvertDfToVfr(df,
                                                ResolvedType.CreateFunction(itemType)));
                                            break;
                                    }
                                }
                                break;

                            case "System.Collections.Generic.HashSet":
                                {
                                    ResolvedType itemType = rootResolvedType.GetEnumerableItemType();
                                    switch (df.FieldName.Value)
                                    {
                                        case "Remove":
                                            possibleRoots.Add(ConvertDfToVfr(df,
                                                ResolvedType.CreateFunction(ResolvedType.Void(), itemType)));
                                            break;

                                        default: break;
                                    }
                                }
                                break;
                        }

                        if (rootResolvedType.IsICollection(context))
                        {
                            ResolvedType itemType = rootResolvedType.GetEnumerableItemType();
                            switch (df.FieldName.Value)
                            {
                                case "Add":
                                    possibleRoots.Add(ConvertDfToVfr(df,
                                        ResolvedType.CreateFunction(ResolvedType.Void(), itemType)));
                                    break;

                                case "Clear":
                                    possibleRoots.Add(ConvertDfToVfr(df,
                                        ResolvedType.CreateFunction(ResolvedType.Void())));
                                    break;

                                case "Contains":
                                    possibleRoots.Add(ConvertDfToVfr(df,
                                        ResolvedType.CreateFunction(ResolvedType.Bool(), itemType)));
                                    break;

                            }
                        }

                        if (rootResolvedType.IsIList(context))
                        {
                            ResolvedType itemType = rootResolvedType.GetEnumerableItemType();
                            switch (df.FieldName.Value)
                            {
                                case "AddRange":
                                    // This is actually just List only, not IList
                                    if (rootResolvedType.FrameworkClass == "System.Collections.Generic.List")
                                    {
                                        possibleRoots.Add(ConvertDfToVfr(df, ResolvedType.CreateFunction(
                                            ResolvedType.Void(),
                                            ResolvedType.CreateEnumerableType(itemType))));
                                    }
                                    break;
                                default:
                                    break;
                            }
                        }

                        if (rootResolvedType.IsIDictionary(context))
                        {
                            ResolvedType keyType = rootResolvedType.Generics[0];
                            ResolvedType valueType = rootResolvedType.Generics[1];
                            switch (df.FieldName.Value)
                            {
                                case "Clear":
                                    possibleRoots.Add(ConvertDfToVfr(df,
                                        ResolvedType.CreateFunction(ResolvedType.Void())));
                                    break;

                                case "ContainsKey":
                                    possibleRoots.Add(ConvertDfToVfr(df,
                                        ResolvedType.CreateFunction(ResolvedType.Bool(), keyType)));
                                    break;

                                case "ContainsValue":
                                    possibleRoots.Add(ConvertDfToVfr(df,
                                        ResolvedType.CreateFunction(ResolvedType.Bool(), valueType)));
                                    break;

                                case "TryGetValue":
                                    possibleRoots.Add(ConvertDfToVfr(df,
                                        ResolvedType.CreateFunction(ResolvedType.Bool(),
                                        new ResolvedType[] {
                                        keyType,
                                        valueType })));
                                    break;

                                default: break;
                            }
                        }

                        switch (df.FieldName.Value)
                        {
                            case "ToString": possibleRoots.Add(ConvertDfToVfr(df, ResolvedType.CreateFunction(ResolvedType.String()))); break;
                            case "Equals": possibleRoots.Add(ConvertDfToVfr(df, ResolvedType.CreateFunction(ResolvedType.Bool(), ResolvedType.Object()))); break;
                            case "GetHashCode": possibleRoots.Add(ConvertDfToVfr(df, ResolvedType.CreateFunction(ResolvedType.Int()))); break;
                            case "GetType": possibleRoots.Add(ConvertDfToVfr(df, ResolvedType.CreateFunction(ResolvedType.CreateFrameworkType("System.Type")))); break;
                        }

                        if (possibleRoots.Count == 0)
                        {
                            string rootType = df.Root.ResolvedType.ToString();
                            // something else that isn't linq, a list, or dictionary
                            throw new System.NotImplementedException("Method name: " + df.FieldName.Value);
                        }
                    }
                    else if (rootResolvedType.PrimitiveType != null)
                    {
                        switch (rootResolvedType.PrimitiveType + "." + df.FieldName)
                        {
                            case "string.Join":
                                throw new System.NotImplementedException();
                            case "string.ToLower":
                            case "string.ToLowerInvariant":
                                possibleRoots.Add(ConvertDfToVfr(df, ResolvedType.CreateFunction(ResolvedType.String())));
                                break;
                            case "string.Split":
                                possibleRoots.Add(ConvertDfToVfr(df, ResolvedType.CreateFunction(
                                    ResolvedType.String(),
                                    new ResolvedType[] { ResolvedType.String(), ResolvedType.CreateArray(ResolvedType.String()) })));
                                possibleRoots.Add(ConvertDfToVfr(df, ResolvedType.CreateFunction(
                                    ResolvedType.String(),
                                    new ResolvedType[] { ResolvedType.Char(), ResolvedType.CreateArray(ResolvedType.String()) })));
                                break;
                            default:
                                throw new System.NotImplementedException();
                        }
                    }
                    else
                    {
                        throw new System.NotImplementedException();
                    }
                }
            }
            else if (this.Root is ConstructorInvocationFragment)
            {
                ConstructorInvocationFragment cif = (ConstructorInvocationFragment)this.Root;
                cif = (ConstructorInvocationFragment)cif.ResolveTypes(context, varScope);

                ClassDefinition cd = cif.Class.CustomType as ClassDefinition;
                if (cd != null)
                {
                    this.ResolvedType = cif.Class;
                    foreach (ConstructorDefinition ctor in cd.Members.OfType<ConstructorDefinition>())
                    {
                        Expression cifw = new ConstructorInvocationFragmentWrapper(cif);
                        cifw.ResolvedType = ResolvedType.CreateFunction(cif.Class, ctor.ResolvedArgTypes);
                        possibleRoots.Add(cifw);
                    }
                }
                else if (cif.Class.FrameworkClass != null)
                {
                    if (FRAMEWORK_CONSTRUCTOR_FUNCTION_SIGNATURES.ContainsKey(cif.Class.FrameworkClass))
                    {
                        foreach (ResolvedType funcType in FRAMEWORK_CONSTRUCTOR_FUNCTION_SIGNATURES[cif.Class.FrameworkClass])
                        {
                            possibleRoots.Add(new ConstructorInvocationFragmentWrapper(cif) { ResolvedType = funcType });
                        }
                    }
                    else
                    {
                        ResolvedType[] generics = cif.Class.Generics;
                        // There's generics. Single out the collection types.
                        switch (cif.Class.FrameworkClass)
                        {
                            case "System.Collections.Generic.List":
                            case "System.Collections.Generic.Stack":
                            case "System.Collections.Generic.Queue":
                            case "System.Collections.Generic.HashSet":
                                // collection with 1 generic

                                // Create an empty collection
                                possibleRoots.Add(new ConstructorInvocationFragmentWrapper(cif)
                                {
                                    ResolvedType = ResolvedType.CreateFunction(cif.Class),
                                });

                                // Create a collection with an enumerable
                                possibleRoots.Add(new ConstructorInvocationFragmentWrapper(cif)
                                {
                                    ResolvedType = ResolvedType.CreateFunction(cif.Class, ResolvedType.CreateEnumerableType(generics[0]))
                                });

                                // Create a collection with default capacity
                                possibleRoots.Add(new ConstructorInvocationFragmentWrapper(cif)
                                {
                                    ResolvedType = ResolvedType.CreateFunction(cif.Class, ResolvedType.Int()),
                                });

                                break;

                            case "System.Collections.Generic.Dictionary":
                                // collection with 2 generics
                                possibleRoots.Add(new ConstructorInvocationFragmentWrapper(cif)
                                {
                                    ResolvedType = ResolvedType.CreateFunction(cif.Class)
                                });
                                if (this.Args.Length == 1)
                                {
                                    throw new System.NotImplementedException();
                                }
                                break;

                            default:
                                throw new ParserException(cif.FirstToken, "Cannot find this framework class: " + cif.Class.FrameworkClass);
                        }
                    }
                }
            }
            else
            {
                throw new System.NotImplementedException();
            }

            List<Expression> possibleFunctionRoots = new List<Expression>();
            List<Expression> filtered = new List<Expression>();
            foreach (Expression possibleRoot in possibleRoots)
            {
                if (possibleRoot.ResolvedType.FrameworkClass != "System.Func")
                {
                    throw new ParserException(this.OpenParen, "This type can't be invoked like a function.");
                }

                if (possibleRoot.ResolvedType.Generics.Length == this.Args.Length + 1)
                {
                    filtered.Add(possibleRoot);
                }
            }
            possibleFunctionRoots = filtered;
            if (possibleFunctionRoots.Count == 0) throw new ParserException(this.FirstToken, "Could not resolve this function");

            List<ResolvedType> argTypes = new List<ResolvedType>();

            for (int i = 0; i < this.Args.Length; ++i)
            {
                Expression arg = this.Args[i];
                if (arg is Lambda)
                {
                    Lambda lambda = (Lambda)arg;
                    List<ResolvedType[]> allCompatibleArgPatterns = new List<ResolvedType[]>();
                    for (int j = 0; j < possibleFunctionRoots.Count; ++j)
                    {
                        ResolvedType expectedArgType = possibleFunctionRoots[j].ResolvedType.Generics[i];
                        if (expectedArgType.PrimitiveType == "object")
                        {
                            throw new ParserException(arg.FirstToken, "Trying to pass in a lambda with no type information in its args into a method that takes in an object, so I can't actually determine what types these args are supposed to be.");
                        }
                        if (expectedArgType.FrameworkClass != "System.Func")
                        {
                            possibleFunctionRoots.RemoveAt(j); // no longer consider this as a possible combination
                            --j;
                            continue;
                        }
                        List<ResolvedType> expectedLambdaArgTypes = new List<ResolvedType>(expectedArgType.Generics);
                        if (expectedLambdaArgTypes.Count - 1 != lambda.Args.Length)
                        {
                            possibleFunctionRoots.RemoveAt(j);
                            --j;
                            continue;
                        }
                        allCompatibleArgPatterns.Add(expectedLambdaArgTypes.ToArray());
                    }

                    if (allCompatibleArgPatterns.Count == 1)
                    {
                        ResolvedType[] expectedArgPatternWinner = allCompatibleArgPatterns[0];

                        arg = ((Lambda)arg).ResolveTypesWithExteriorHint(context, varScope, expectedArgPatternWinner);

                        // If the outgoing return type is not known, then scrape it from the resolved lambda, which
                        // is now aware of its own return type from within the code.
                        if (expectedArgPatternWinner[expectedArgPatternWinner.Length - 1] == null)
                        {
                            ResolvedType returnTypeFromInsideLambda = arg.ResolvedType.Generics[arg.ResolvedType.Generics.Length - 1];
                            expectedArgPatternWinner[expectedArgPatternWinner.Length - 1] = returnTypeFromInsideLambda;
                            possibleFunctionRoots[0].ResolvedType.RecursivelyApplyATypeToAllNulls(returnTypeFromInsideLambda);
                        }
                    }
                    else
                    {
                        // TODO: check to see if all the possible expected arg types are the same, in which case it should be treated as just 1
                        throw new ParserException(this.OpenParen, "This function invocation is ambiguous. Multiplie lambdas apply");
                    }
                }
                else
                {
                    arg = arg.ResolveTypes(context, varScope);
                }
                this.Args[i] = arg;
                argTypes.Add(this.Args[i].ResolvedType);
            }

            foreach (Expression possibleRoot in possibleFunctionRoots)
            {
                ResolvedType[] expectedArgTypes = possibleRoot.ResolvedType.Generics; // has an extra type at the end for the return type but since we're looping through the args length, this won't be an issue
                bool isMatch = true;
                for (int i = 0; i < this.Args.Length; ++i)
                {
                    if (!this.Args[i].ResolvedType.CanBeAssignedTo(expectedArgTypes[i], context))
                    {
                        isMatch = false;
                        break;
                    }
                }

                if (isMatch)
                {
                    this.Root = possibleRoot;
                    if (this.Root is ConstructorInvocationFragmentWrapper)
                    {
                        ConstructorInvocationFragmentWrapper cifw = (ConstructorInvocationFragmentWrapper)this.Root;
                        this.Root = cifw.InnerFragment;
                        this.Root.ResolvedType = cifw.ResolvedType;
                        cifw.InnerFragment.ResolveTypesForInitialData(context, varScope);
                    }
                    ResolvedType funcType = this.Root.ResolvedType;
                    this.ResolvedType = funcType.Generics[funcType.Generics.Length - 1];
                    return this;
                }
            }

            throw new ParserException(this.OpenParen, "No acceptable function signature could be found to match the args.");
        }

        private static VerifiedFieldReference ConvertDfToLinqVfr(DotField df, ResolvedType type)
        {
            VerifiedFieldReference vfr = ConvertDfToVfr(df, type);
            vfr.Type = MethodRefType.LINQ;
            return vfr;
        }

        private static VerifiedFieldReference ConvertDfToVfr(DotField df, ResolvedType type)
        {
            VerifiedFieldReference vfr = new VerifiedFieldReference(df.FirstToken, df.Parent, df.FieldName, df.Root, null);
            vfr.ResolvedType = type;
            return vfr;
        }

        private static ResolvedType CreateConstructorFuncWithArgs(string className, params ResolvedType[] argTypes)
        {
            return ResolvedType.CreateFunction(
                ResolvedType.CreateFrameworkType(className),
                argTypes);
        }

        private static ResolvedType[] CreateStandardExceptionConstructor(string name)
        {
            return new ResolvedType[] {
                CreateConstructorFuncWithArgs(name),
                CreateConstructorFuncWithArgs(name, ResolvedType.String())
            };
        }

        private static readonly Dictionary<string, ResolvedType[]> FRAMEWORK_CONSTRUCTOR_FUNCTION_SIGNATURES = new Dictionary<string, ResolvedType[]>() {
            { "System.Exception", CreateStandardExceptionConstructor("System.Exception") },
            { "System.NotImplementedException", CreateStandardExceptionConstructor("System.NotImplementedException") },
            { "System.InvalidOperationException", CreateStandardExceptionConstructor("System.InvalidOperationException") },
            {
                "System.Text.StringBuilder",
                new ResolvedType[] {
                    CreateConstructorFuncWithArgs("System.Text.StringBuilder"),
                }
            },
        };
    }
}
