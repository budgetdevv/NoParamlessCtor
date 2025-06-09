using Microsoft.CodeAnalysis;
using NoParamlessCtor.SourceGenerator.Helpers;

namespace NoParamlessCtor.SourceGenerator.CodeGeneration
{
    public struct NamespaceBlock
    {
        public readonly ISymbol NamespaceSymbol;

        public readonly string Code;

        public NamespaceBlock(StructBlock structBlock)
        {
            var namespaceSymbol = NamespaceSymbol = structBlock
                .TypeSymbol
                .ContainingNamespace;

            var namespaceText = namespaceSymbol.ToDisplayString();

            Code =
            $$"""
            namespace {{namespaceText}}
            {
                {{structBlock.Code.IndentTrailing()}}
            }
            """;
        }
    }
}