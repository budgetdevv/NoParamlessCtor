using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace NoParamlessCtor.SourceGenerator.Helpers
{
    public static class CodeGenerationHelpers
    {
        public static string ToText(this Accessibility modifier)
        {
            return modifier switch
            {
                Accessibility.Public => "public",
                Accessibility.Private => "private",
                Accessibility.Protected => "protected",
                Accessibility.Internal => "internal",
                Accessibility.ProtectedAndInternal => "protected internal",
                _ => throw new ArgumentOutOfRangeException(nameof(modifier), modifier, null)
            };
        }

        public static string ConstructNamespace(this ITypeSymbol typeSymbol)
        {
            return typeSymbol.ContainingNamespace.ToDisplayString();
        }

        public static string IndentTrailing(this string text)
        {
            var lines = text.Split('\n');

            return string.Join($"\n{Constants.INDENTATION}", lines);
        }

        public static string GetFullyQualifiedName(
            this ITypeSymbol typeSymbol,
            SymbolDisplayGlobalNamespaceStyle globalNamespaceStyle = SymbolDisplayGlobalNamespaceStyle.Omitted,
            SymbolDisplayGenericsOptions genericsOptions = SymbolDisplayGenericsOptions.IncludeTypeParameters)
        {
            var format = new SymbolDisplayFormat(
                globalNamespaceStyle: globalNamespaceStyle,
                typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces,
                genericsOptions: genericsOptions,
                miscellaneousOptions:
                    SymbolDisplayMiscellaneousOptions.EscapeKeywordIdentifiers |
                    SymbolDisplayMiscellaneousOptions.UseSpecialTypes
            );

            return typeSymbol.ToDisplayString(format);
        }

        public static bool IsPartial(this ITypeSymbol typeSymbol)
        {
            return typeSymbol.DeclaringSyntaxReferences
                .Select(x => x.GetSyntax())
                .OfType<ClassDeclarationSyntax>()
                .Any(x => x.Modifiers.Any(y => y.ValueText == "partial"));
        }

        public static bool ContainsKeyword(
            this IPropertySymbol property,
            string keyword)
        {
            return property.ContainsKeyword([ keyword ]);
        }

        public static bool ContainsKeyword(
            this IPropertySymbol property,
            string[] keywords)
        {
            return property.DeclaringSyntaxReferences
                .Select(x => x.GetSyntax())
                .OfType<PropertyDeclarationSyntax>()
                .Any(x =>
                {
                    var modifiers = x.Modifiers.Select(y => y.ValueText);

                    return keywords.All(y => modifiers.Contains(y));
                });
        }

        public static bool IsPartial(this IPropertySymbol property)
        {
            return property.ContainsKeyword("partial");
        }

        public static bool HasRequiredKeyword(this IPropertySymbol property)
        {
            return property.ContainsKeyword("required");
        }

        public static bool ContainsKeyword(this ParameterSyntax parameter, string keyword)
        {
            return parameter.Modifiers.Any(x => x.ValueText == keyword);
        }

        public static bool ContainsKeyword(this ParameterSyntax parameter, string[] keywords)
        {
            return parameter.Modifiers
                .Select(x => x.ValueText)
                .Any(keywords.Contains);
        }

        public static bool ContainsKeyword(this StructDeclarationSyntax declarationSyntax, string[] keywords)
        {
            return declarationSyntax
                .Modifiers
                .Select(x => x.ValueText)
                .Any(keywords.Contains);
        }

        public static bool ContainsKeyword(this StructDeclarationSyntax declarationSyntax, string keyword)
        {
            return declarationSyntax
                .Modifiers
                .Any(x => x.ValueText == keyword);
        }

        public static bool ContainsAttributes(this ISymbol symbol, string[] attributeNames)
        {
            return symbol
                .GetAttributes()
                .Any(
                    x => attributeNames.Contains(x.AttributeClass?.Name)
                );
        }

        public static bool ContainsAttribute(this ISymbol symbol, string attributeName)
        {
            return symbol
                .GetAttributes()
                .Any(
                    x => x.AttributeClass?.Name == attributeName
                );
        }

        public static bool HasRequiredAttribute(this IPropertySymbol property)
        {
            return property.ContainsAttribute(nameof(RequiredAttributeAttribute));
        }

        public static bool IsNullable(this IPropertySymbol property)
        {
            return property.NullableAnnotation == NullableAnnotation.Annotated;
        }

        public static IEnumerable<ParameterSyntax> GetPrimaryConstructorParams(this StructDeclarationSyntax declaration)
        {
            if (declaration.ParameterList == null)
            {
                yield break;
            }

            foreach (var parameter in declaration.ParameterList.Parameters)
            {
                yield return parameter;
            }
        }
    }
}