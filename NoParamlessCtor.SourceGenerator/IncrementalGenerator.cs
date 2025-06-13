using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using NoParamlessCtor.Shared.Attributes;
using NoParamlessCtor.SourceGenerator.CodeGeneration;
using NoParamlessCtor.SourceGenerator.Helpers;

namespace NoParamlessCtor.SourceGenerator
{
    [Generator]
	public class IncrementalGenerator: IIncrementalGenerator
    {
        private static readonly string NO_PARAM_CTOR_ATTRIBUTE_NAME = nameof(NoParamlessCtorAttribute)
            .Replace(nameof(Attribute), string.Empty);

		public void Initialize(IncrementalGeneratorInitializationContext context)
		{
            var structDeclarations = context.SyntaxProvider.CreateSyntaxProvider(
               predicate: Predicate,
               transform: GetStructTypeSymbols
            ).Collect();
            
            context.RegisterSourceOutput(structDeclarations, GenerateSource);

            return;

            static bool Predicate(SyntaxNode node, CancellationToken cancellationToken)
            {
                if (node is not StructDeclarationSyntax structDeclaration ||
                    !structDeclaration.Modifiers.Any(x => x.IsKind(SyntaxKind.PartialKeyword)))
                {
                    return false;
                }

                return structDeclaration
                    .AttributeLists
                    .SelectMany(x => x.Attributes)
                    .Any(x => x.Name.ToString() == NO_PARAM_CTOR_ATTRIBUTE_NAME);
            }

            static (StructDeclarationSyntax declaration, ITypeSymbol? typeSymbol, SemanticModel semanticModel) GetStructTypeSymbols(
                GeneratorSyntaxContext context,
                CancellationToken cancellationToken)
            {
                var declaration = (StructDeclarationSyntax) context.Node;

                var semanticModel = context.SemanticModel;

                if (ModelExtensions.GetDeclaredSymbol(semanticModel, declaration, cancellationToken) is ITypeSymbol typeSymbol)
                {
                    return (declaration, typeSymbol, semanticModel);
                }

                return (declaration, null, semanticModel);
            }
        }

        private static void GenerateSource(
            SourceProductionContext context,
            ImmutableArray<(StructDeclarationSyntax declaration, ITypeSymbol? typeSymbol, SemanticModel semanticModel)> typeSymbols)
        {
            foreach (var (declaration, typeSymbol, semanticModel) in typeSymbols)
            {
                if (typeSymbol == null)
                {
                    continue;
                }

                var structBody = new StructBody();

                var primaryCtorParams = declaration
                    .GetPrimaryConstructorParams()
                    .ToArray();

                string paramlessCtorSuffix;

                if (primaryCtorParams.Length == 0)
                {
                    paramlessCtorSuffix = string.Empty;
                }

                else
                {
                    var paramTexts = new List<string>();

                    foreach (var primaryCtorParam in primaryCtorParams)
                    {
                        var isRef = primaryCtorParam.ContainsKeyword("ref");

                        var isIn = primaryCtorParam.ContainsKeyword("in");

                        string addedText;

                        if (isRef || isIn)
                        {
                            var paramTypeInfo = semanticModel
                                .GetTypeInfo(primaryCtorParam.Type!)
                                .Type!;

                            var paramTypeFQN = paramTypeInfo.GetFullyQualifiedName();

                            var keyword = isRef ? "ref" : "in";

                            addedText = $"{keyword} Unsafe.NullRef<{paramTypeFQN}>()";
                        }

                        else
                        {
                            addedText = "default";
                        }

                        paramTexts.Add(addedText);
                    }

                    paramlessCtorSuffix = string.Join(
                        ", ",
                        paramTexts
                    );

                    paramlessCtorSuffix = $": this({paramlessCtorSuffix})";
                }

                var structName = typeSymbol.Name;

                structBody.AppendCode(
                $$"""
                [Obsolete("Do not use paramless constructor", error: true)]
                public {{structName}}(){{paramlessCtorSuffix}} { }
                """);

                var structBlock = new StructBlock(
                    typeSymbol,
                    structBody
                );

                var namespaceBlock = new NamespaceBlock(structBlock);

                context.AddSource(
                    structBlock.GenerateFileName(),
                    SourceText.From(namespaceBlock.Code, Encoding.UTF8)
                );
            }

            const string GLOBAL_IMPORTS =
            """
            // Used by System.ObsoleteAttribute
            global using System;
            
            // Used by Unsafe.NullRef()
            global using System.Runtime.CompilerServices;
            """;

            context.AddSource(
                "NoParamlessCtorGlobalImports.g.cs",
                SourceText.From(GLOBAL_IMPORTS, Encoding.UTF8)
            );
        }
    }
}

