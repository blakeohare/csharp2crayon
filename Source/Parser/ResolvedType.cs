using CSharp2Crayon.Parser.Nodes;
using System.Collections.Generic;
using System.Linq;

namespace CSharp2Crayon.Parser
{
    public class ResolvedType
    {
        public TopLevelEntity CustomType { get; set; }
        public string FrameworkClass { get; set; }
        public string PrimitiveType { get; set; }
        public bool IsVoid { get; private set; }
        public bool IsArray { get; private set; }
        public bool IsNullable { get; private set; }
        public ResolvedType[] Generics { get; private set; }

        public bool IsNull { get; private set; } // The null type is for expressions and works like a generic object reference except it can be assigned to more specific things.

        private static readonly ResolvedType IENUMERABLE = new ResolvedType() { FrameworkClass = "System.Collections.Generic.IEnumerable" };
        private static readonly ResolvedType[] EMPTY_GENERICS = new ResolvedType[0];
        public bool IsEnumerable(ParserContext ctx)
        {
            if (this.Generics.Length != 1) return false;
            if (this.IsArray) return true;
            if (this.FrameworkClass != null)
            {
                return ResolvedType.IsXASubclassOfY(this.FrameworkClass, "System.Collections.Generic.IEnumerable", ctx);
            }
            if (this.CustomType != null)
            {
                return ResolvedType.IsXASubclassOfY(this.CustomType.FullyQualifiedName, "System.Collections.Generic.IEnumerable", ctx);
            }
            if (this.IsNullable)
            {
                return false;
            }
            if (this.PrimitiveType != null)
            {
                return false;
            }
            throw new System.NotImplementedException();
        }

        public override string ToString()
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            ToStringImpl(sb);
            return sb.ToString();
        }

        private void ToStringImpl(System.Text.StringBuilder sb)
        {
            if (this.IsArray)
            {
                this.Generics[0].ToStringImpl(sb);
                sb.Append("[]");
                return;
            }

            if (this.IsNullable)
            {
                this.Generics[0].ToStringImpl(sb);
                sb.Append("?");
                return;
            }

            if (this.FrameworkClass != null)
            {
                sb.Append(this.FrameworkClass);
            }
            else if (this.PrimitiveType != null)
            {
                sb.Append(this.PrimitiveType);
            }
            else if (this.CustomType != null)
            {
                sb.Append(this.CustomType.FullyQualifiedName);
            }

            if (this.Generics.Length > 0)
            {
                sb.Append("<");
                for (int i = 0; i < this.Generics.Length; ++i)
                {
                    if (i > 0) sb.Append(", ");
                    if (this.Generics[i] == null)
                    {
                        sb.Append("null");
                    }
                    else
                    {
                        this.Generics[i].ToStringImpl(sb);
                    }
                }
                sb.Append(">");
            }
        }

        public void RecursivelyApplyATypeToAllNulls(ResolvedType replacer)
        {
            for (int i = 0; i < this.Generics.Length; ++i)
            {
                if (this.Generics[i] == null)
                {
                    this.Generics[i] = replacer;
                }
                else
                {
                    this.Generics[i].RecursivelyApplyATypeToAllNulls(replacer);
                }
            }
        }

        public bool IsICollection(ParserContext ctx)
        {
            if (this.Generics.Length != 1) return false;
            if (this.FrameworkClass != null)
            {
                return ResolvedType.IsXASubclassOfY(this.FrameworkClass, "System.Collections.Generic.ICollection", ctx);
            }
            return false;
        }

        public bool IsIList(ParserContext ctx)
        {
            if (this.Generics.Length != 1) return false;
            if (this.FrameworkClass != null)
            {
                return ResolvedType.IsXASubclassOfY(this.FrameworkClass, "System.Collections.Generic.IList", ctx);
            }
            return false;
        }

        public bool IsIDictionary(ParserContext ctx)
        {
            if (this.Generics.Length != 2) return false;
            if (this.FrameworkClass != null)
            {
                return ResolvedType.IsXASubclassOfY(this.FrameworkClass, "System.Collections.Generic.IDictionary", ctx);
            }
            return false;
        }

        public ResolvedType GetEnumerableItemType()
        {
            if (this.IsArray) return this.Generics[0];
            switch (this.FrameworkClass)
            {
                case "System.Collections.Generic.HashSet":
                case "System.Collections.Generic.Stack":
                case "System.Collections.Generic.Queue":
                case "System.Collections.Generic.List":
                case "System.Collections.Generic.IList":
                case "System.Collections.Generic.ICollection":
                case "System.Collections.Generic.IEnumerable":
                    return this.Generics[0];
            }

            throw new System.NotImplementedException();
        }

        public static ResolvedType FromClass(ClassLikeDefinition cd)
        {
            return new ResolvedType()
            {
                CustomType = cd,
                Generics = EMPTY_GENERICS,
            };
        }

        private static readonly Dictionary<string, ResolvedType> primitiveCache = new Dictionary<string, ResolvedType>();
        public static ResolvedType GetPrimitiveType(string name)
        {
            ResolvedType output;
            if (!primitiveCache.TryGetValue(name, out output))
            {
                output = ResolvedType.CreatePrimitive(name, null);
                primitiveCache[name] = output;
            }
            return output;
        }

        public static ResolvedType String() { return GetPrimitiveType("string"); }
        public static ResolvedType Int() { return GetPrimitiveType("int"); }
        public static ResolvedType Bool() { return GetPrimitiveType("bool"); }
        public static ResolvedType Object() { return GetPrimitiveType("object"); }
        public static ResolvedType Void() { ResolvedType t = GetPrimitiveType("void"); t.IsVoid = true; return t; }

        public static ResolvedType CreateArray(ResolvedType itemType)
        {
            return new ResolvedType()
            {
                IsArray = true,
                Generics = new ResolvedType[] { itemType }
            };
        }

        public static ResolvedType CreateFunction(ResolvedType returnType)
        {
            return new ResolvedType()
            {
                FrameworkClass = "System.Func",
                Generics = new ResolvedType[] { returnType },
            };
        }

        public static ResolvedType CreateFunctionPointerType(ResolvedType returnType, ResolvedType[] args)
        {
            List<ResolvedType> generics = new List<ResolvedType>(args);
            generics.Add(returnType);
            return new ResolvedType()
            {
                FrameworkClass = "System.Func",
                Generics = generics.ToArray(),
            };
        }

        public static ResolvedType CreateFunctionPointerType(ResolvedType returnType, ResolvedType singleArgumentType)
        {
            return new ResolvedType()
            {
                FrameworkClass = "System.Func",
                Generics = new ResolvedType[] { singleArgumentType, returnType },
            };
        }

        public static ResolvedType CreateEnumerableType(ResolvedType thingYoureEnumerating)
        {
            return new ResolvedType()
            {
                FrameworkClass = "System.Collections.Generic.IEnumerable",
                Generics = new ResolvedType[] { thingYoureEnumerating },
            };
        }

        public static ResolvedType CreateNull()
        {
            return new ResolvedType()
            {
                Generics = EMPTY_GENERICS,
                PrimitiveType = "null",
                IsNull = true
            };
        }

        public static ResolvedType CreateFrameworkType(string name)
        {
            return new ResolvedType()
            {
                Generics = EMPTY_GENERICS,
                FrameworkClass = name,
            };
        }

        public static ResolvedType CreateFrameworkType(string[] parts)
        {
            return new ResolvedType()
            {
                Generics = EMPTY_GENERICS,
                FrameworkClass = string.Join('.', parts),
            };
        }

        public static ResolvedType CreatePrimitive(string name, Token throwToken)
        {
            if (PRIMITIVE_TYPES.Contains(name))
            {
                return new ResolvedType()
                {
                    Generics = EMPTY_GENERICS,
                    IsVoid = name == "void",
                    PrimitiveType = name,
                };
            }

            throw new ParserException(throwToken, "Unrecognized primitive type: " + name);
        }

        public static ResolvedType Create(CSharpType type, string[] prefixes, ParserContext context)
        {
            ResolvedType[] generics = EMPTY_GENERICS;
            if (type.Generics.Length > 0)
            {
                generics = type.Generics.Select(g => Create(g, prefixes, context)).ToArray();
            }

            string typeString = type.RootTypeString;
            if (typeString == "[" || typeString == "?")
            {
                return new ResolvedType()
                {
                    IsArray = typeString == "[",
                    IsNullable = typeString == "?",
                    Generics = generics,
                };
            }

            if (PRIMITIVE_TYPES.Contains(typeString))
            {
                return new ResolvedType()
                {
                    Generics = EMPTY_GENERICS,
                    PrimitiveType = typeString,
                    IsVoid = typeString == "void",
                };
            }
            foreach (string prefix in prefixes)
            {
                string fullyQualifiedName = prefix + typeString;
                if (FRAMEWORK_CLASSES_AND_PARENTS.ContainsKey(fullyQualifiedName))
                {
                    return new ResolvedType()
                    {
                        FrameworkClass = fullyQualifiedName,
                        Generics = generics,
                    };
                }
                TopLevelEntity tle = context.DoLookup(fullyQualifiedName);
                if (tle != null)
                {
                    return new ResolvedType()
                    {
                        CustomType = tle,
                        Generics = generics,
                    };
                }
            }

            return null;
        }

        public bool CanBeAssignedTo(ResolvedType otherType, ParserContext parserContext)
        {
            if (otherType.IsVoid || this.IsVoid) return false;

            if (otherType == this) return true;

            if (this.IsSameAs(otherType)) return true;

            if (this.Generics.Length != otherType.Generics.Length) return false;

            if (otherType.PrimitiveType == "object") return true;

            if (this.IsNull)
            {
                if (otherType.PrimitiveType != null)
                {
                    switch (otherType.PrimitiveType)
                    {
                        case "string":
                        case "object":
                            return true;
                        default:
                            return false;
                    }
                }
                return true;
            }

            for (int i = 0; i < this.Generics.Length; ++i)
            {
                if (!this.Generics[i].IsSameAs(otherType.Generics[i]))
                {
                    return false;
                }
            }

            string thisTypeString = this.CustomType == null ? this.FrameworkClass : this.CustomType.FullyQualifiedName;
            string otherTypeString = otherType.CustomType == null ? otherType.FrameworkClass : otherType.CustomType.FullyQualifiedName;

            if (thisTypeString == null) throw new System.NotImplementedException(); // what is this?
            if (otherTypeString == null) throw new System.NotImplementedException(); // what is this?

            if (thisTypeString == otherTypeString) return true;

            return IsXASubclassOfY(thisTypeString, otherTypeString, parserContext);
        }

        private static Dictionary<string, HashSet<string>> allAncestorsOfClass = new Dictionary<string, HashSet<string>>();

        private static bool IsXASubclassOfY(string x, string y, ParserContext parserContext)
        {
            HashSet<string> lookup;
            if (!allAncestorsOfClass.TryGetValue(x, out lookup))
            {
                lookup = BuildAncestorLookupForClass(x, parserContext);
                allAncestorsOfClass[x] = lookup;
            }
            return lookup.Contains(y);
        }

        private static HashSet<string> BuildAncestorLookupForClass(string className, ParserContext parserContext)
        {
            HashSet<string> output = new HashSet<string>();
            output.Add("object");
            BuildAncestorLookupForClassImpl(className, parserContext, output);
            return output;
        }

        private static void BuildAncestorLookupForClassImpl(string className, ParserContext parserContext, HashSet<string> lookup)
        {
            lookup.Add(className);
            if (FRAMEWORK_CLASSES_AND_PARENTS.ContainsKey(className))
            {
                foreach (string parent in FRAMEWORK_CLASSES_AND_PARENTS[className])
                {
                    BuildAncestorLookupForClassImpl(parent, parserContext, lookup);
                }
            }
            else
            {
                TopLevelEntity tle = parserContext.DoLookup(className);
                if (tle is ClassLikeDefinition)
                {
                    ClassLikeDefinition cd = (ClassLikeDefinition)tle;
                    foreach (ResolvedType parentType in cd.ParentClasses)
                    {
                        if (parentType.CustomType != null)
                        {
                            BuildAncestorLookupForClassImpl(parentType.CustomType.FullyQualifiedName, parserContext, lookup);
                        }
                        else if (parentType.FrameworkClass != null)
                        {
                            BuildAncestorLookupForClassImpl(parentType.FrameworkClass, parserContext, lookup);
                        }
                        else
                        {
                            throw new System.NotImplementedException();
                        }
                    }
                }
                else
                {
                    throw new System.NotImplementedException();
                }
            }
        }

        public bool IsSameAs(ResolvedType otherType)
        {
            if (otherType.IsVoid || this.IsVoid) return false;

            if ((this.IsNullable && otherType.IsNullable) ||
                (this.IsArray && otherType.IsArray))
            {
                return this.Generics[0].IsSameAs(otherType.Generics[0]);
            }

            if (this.FrameworkClass == otherType.FrameworkClass &&
                this.CustomType == otherType.CustomType &&
                this.PrimitiveType == otherType.PrimitiveType)
            {
                if (this.Generics.Length == otherType.Generics.Length)
                {
                    int gLength = this.Generics.Length;
                    for (int i = 0; i < gLength; ++i)
                    {
                        if (!this.Generics[i].IsSameAs(otherType.Generics[i]))
                        {
                            return false;
                        }
                    }
                    return true;
                }
            }

            return false;
        }

        private static HashSet<string> PRIMITIVE_TYPES = new HashSet<string>() {
            "int",
            "double",
            "float",
            "byte",
            "long",
            "char",
            "string",
            "bool",
            "object",

            "void",
        };

        private static Dictionary<string, HashSet<string>> CLASS_ALL_DIRECT_PARENTS = new Dictionary<string, HashSet<string>>();
        private static Dictionary<string, HashSet<string>> CLASS_ALL_PARENTS = new Dictionary<string, HashSet<string>>();

        private static Dictionary<string, string[]> FRAMEWORK_CLASSES_AND_PARENTS = new Dictionary<string, string[]>() {
            // Note that there's also a constructor list at the bottom of FunctionInvocation.cs
            { "System.Collections.Generic.Dictionary", "System.Collections.Generic.IDictionary".Split(',') },
            { "System.Collections.Generic.HashSet", "System.Collections.Generic.ICollection".Split(',') },
            { "System.Collections.Generic.ICollection", "System.Collections.Generic.IEnumerable".Split(',') },
            { "System.Collections.Generic.IComparer", new string[0] },
            { "System.Collections.Generic.IDictionary", "System.Collections.Generic.ICollection".Split(',') },
            { "System.Collections.Generic.IEnumerable", new string[0] },
            { "System.Collections.Generic.IList", "System.Collections.Generic.ICollection".Split(',') },
            { "System.Collections.Generic.List", "System.Collections.Generic.IList".Split(',') },
            { "System.Collections.Generic.Queue", "System.Collections.Generic.IList".Split(',') },
            { "System.Collections.Generic.Stack", "System.Collections.Generic.IList".Split(',') },
            { "System.Exception", new string[0] },
            { "System.Func", new string[0] },
            { "System.IDisposable", new string[0] },
            { "System.InvalidOperationException", "System.Exception".Split(',') },
            { "System.NotImplementedException", "System.Exception".Split(',') },
            { "System.Random", new string[0] },
            { "System.Reflection.Assembly", new string[0] },
            { "System.Text.StringBuilder", new string[0] },
            { "System.Tuple", new string[0] },

            { "Common.JsonLookup", new string[0] },
            { "Common.Multimap", "System.Collections.Generic.IDictionary".Split(',') },
            { "Common.Pair", new string[0] },
            { "Common.SystemBitmap", new string[0] },
        };
    }
}
