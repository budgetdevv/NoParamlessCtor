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
            switch ((modifier))
            {
                case Accessibility.Public:
                    return "public";

                case Accessibility.Private:
                    return "private";

                case Accessibility.Protected:
                    return "protected";

                case Accessibility.Internal:
                    return "internal";

                case Accessibility.ProtectedAndInternal:
                    return "protected internal";

                // TODO: Figure this out
                // return "private protected";
                default:
                    throw new ArgumentOutOfRangeException(nameof(modifier), modifier, null);
            }
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
            bool includeGlobal = false)
        {
            var fqn = typeSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);

            const string GLOBAL_PREFIX = "global::";

            if (!includeGlobal && fqn.StartsWith(GLOBAL_PREFIX))
            {
                fqn = fqn.AsSpan(GLOBAL_PREFIX.Length).ToString();
            }

            return fqn;
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
            return property.ContainsKeywords([ keyword ]);
        }

        public static bool ContainsKeywords(
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