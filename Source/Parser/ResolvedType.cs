using CSharp2Crayon.Parser.Nodes;
using System;
using System.Collections.Generic;

namespace CSharp2Crayon.Parser
{
    public class ResolvedType
    {
        public TopLevelEntity CustomType { get; set; }
        public string FrameworkClass { get; set; }
        public string PrimitiveType { get; set; }

        public static ResolvedType Create(CSharpType type, string[] prefixes, ParserContext context)
        {
            if (PRIMITIVE_TYPES.Contains(type.RootTypeString))
            {
                return new ResolvedType()
                {
                    PrimitiveType = type.RootTypeString
                };
            }
            foreach (string prefix in prefixes)
            {
                string fullyQualifiedName = prefix + type.RootTypeString;
                if (FRAMEWORK_CLASSES.Contains(fullyQualifiedName))
                {
                    return new ResolvedType()
                    {
                        FrameworkClass = fullyQualifiedName
                    };
                }
                TopLevelEntity tle = context.DoLookup(fullyQualifiedName);
                if (tle != null)
                {
                    return new ResolvedType()
                    {
                        CustomType = tle,
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
            "string",
            "bool",
            "object",
        };

        private static HashSet<string> FRAMEWORK_CLASSES = new HashSet<string>() {
            "System.Collections.Generic.Dictionary",
            "System.Collections.Generic.HashSet",
            "System.Collections.Generic.IComparer",
            "System.Collections.Generic.List",
            "System.Exception",
            "System.IDisposable",
        };
    }
}
