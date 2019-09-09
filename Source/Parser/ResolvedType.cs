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
        private static readonly ResolvedType[] EMPTY_GENERICS = new ResolvedType[0];

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
                    PrimitiveType = typeString,
                    IsVoid = typeString == "void",
                };
            }
            foreach (string prefix in prefixes)
            {
                string fullyQualifiedName = prefix + typeString;
                if (FRAMEWORK_CLASSES.Contains(fullyQualifiedName))
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

        private static HashSet<string> FRAMEWORK_CLASSES = new HashSet<string>() {
            "System.Collections.Generic.Dictionary",
            "System.Collections.Generic.HashSet",
            "System.Collections.Generic.ICollection",
            "System.Collections.Generic.IComparer",
            "System.Collections.Generic.IDictionary",
            "System.Collections.Generic.IEnumerable",
            "System.Collections.Generic.IList",
            "System.Collections.Generic.List",
            "System.Collections.Generic.Queue",
            "System.Collections.Generic.Stack",
            "System.Exception",
            "System.Func",
            "System.IDisposable",
            "System.Random",
            "System.Reflection.Assembly",
            "System.Text.StringBuilder",
            "System.Tuple",

            "Common.JsonLookup",
            "Common.Multimap",
            "Common.Pair",
            "Common.SystemBitmap",
        };
    }
}
