using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NoParamlessCtor.SourceGenerator.Helpers;

namespace NoParamlessCtor.SourceGenerator.CodeGeneration
{
    public readonly struct StructBlock
    {
        public readonly TypeDeclarationSyntax DeclarationSyntax;

        public readonly ITypeSymbol TypeSymbol;

        public readonly string Code;

        public readonly IReadOnlyList<string> GenericParamNames;

        public bool IsGenericType => GenericParamNames.Count != 0;

        public StructBlock(
            TypeDeclarationSyntax declarationSyntax,
            INamedTypeSymbol typeSymbol,
            StructBody body)
        {
            DeclarationSyntax = declarationSyntax;

            TypeSymbol = typeSymbol;

            TypeBlock.GetTypeMetadata(
                declarationSyntax,
                typeSymbol,
                out var accessModifier,
                out var structNameText,
                out GenericParamNames,
                out var genericParamsText,
                out var isUnsafe
            );

            var blockKind = TypeBlock.GetBlockKind(declarationSyntax);

            Code = TypeBlock.GenerateCode(
                blockKind,
                declarationSyntax,
                typeSymbol,
                accessModifier,
                isUnsafe,
                structNameText,
                genericParamsText,
                body.Code.ToString()
            );
        }

        public string GenerateFileName()
        {
            var fqn = TypeSymbol.GetFullyQualifiedName(
                genericsOptions: SymbolDisplayGenericsOptions.None
            );

            string genericParamsSuffix;

            if (IsGenericType)
            {
                genericParamsSuffix = $$"""{{{string.Join(", ", GenericParamNames)}}}""";
            }

            else
            {
                genericParamsSuffix = string.Empty;
            }

            return $"{fqn}{genericParamsSuffix}.g.cs";
        }
    }
}