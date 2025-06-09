using System;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using NoParamlessCtor.SourceGenerator.Attributes;
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

            static (StructDeclarationSyntax declaration, ITypeSymbol? typeSymbol) GetStructTypeSymbols(
                GeneratorSyntaxContext context,
                CancellationToken cancellationToken)
            {
                var declaration = (StructDeclarationSyntax) context.Node;

                if (ModelExtensions.GetDeclaredSymbol(context.SemanticModel, declaration, cancellationToken) is ITypeSymbol typeSymbol)
                {
                    return (declaration, typeSymbol);
                }

                return (declaration, null);
            }
        }

        private static void GenerateSource(
            SourceProductionContext context,
            ImmutableArray<(StructDeclarationSyntax declaration, ITypeSymbol? typeSymbol)> typeSymbols)
        {
            // var codeByType = new Dictionary<ITypeSymbol, NamespaceBlock>(
            //     comparer: SymbolEqualityComparer.Default
            // );

            foreach (var (declaration, typeSymbol) in typeSymbols)
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
                    paramlessCtorSuffix = string.Join(
                        ", ",
                        primaryCtorParams.Select(x => "default")
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

                // codeByType.Add(typeSymbol, namespaceBlock);

                context.AddSource(
                    $"{typeSymbol.GetFullyQualifiedName()}.g.cs",
                    SourceText.From(namespaceBlock.Code, Encoding.UTF8)
                );
            }

            const string GLOBAL_IMPORTS =
            """
            global using System;
            """;

            context.AddSource(
                "NoParamlessCtorGlobalImports.g.cs",
                SourceText.From(GLOBAL_IMPORTS, Encoding.UTF8)
            );
        }
    }
}

