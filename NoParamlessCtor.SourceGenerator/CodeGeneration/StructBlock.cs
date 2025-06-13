using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using NoParamlessCtor.SourceGenerator.Helpers;

namespace NoParamlessCtor.SourceGenerator.CodeGeneration
{
    public readonly struct StructBlock
    {
        public readonly ITypeSymbol TypeSymbol;

        public readonly string Code;

        public readonly IReadOnlyList<string> GenericParamNames;

        public bool IsGenericType => GenericParamNames.Count != 0;

        public StructBlock(ITypeSymbol typeSymbol, StructBody body)
        {
            TypeSymbol = typeSymbol;

            var accessModifier = typeSymbol.DeclaredAccessibility.ToText();

            var structNameText = typeSymbol.Name;

            var interfaces = typeSymbol.Interfaces;

            string interfacesText;

            if (interfaces.Length != 0)
            {
                interfacesText = ": ";

                const string SUFFIX = ", ";

                foreach (var @interface in interfaces)
                {
                    interfacesText += $"{@interface.ToDisplayString()}{SUFFIX}";
                }

                interfacesText = interfacesText.AsSpan(0, interfacesText.Length - SUFFIX.Length).ToString();
            }

            else
            {
                interfacesText = string.Empty;
            }

            List<string> genericParamNames;

            if (typeSymbol is INamedTypeSymbol namedTypeSymbol)
            {
                var typeParams = namedTypeSymbol.TypeParameters;

                genericParamNames = new List<string>(typeParams.Length);

                GenericParamNames = genericParamNames;

                foreach (var genericParam in typeParams)
                {
                    genericParamNames.Add(genericParam.GetFullyQualifiedName());
                }
            }

            else
            {
                genericParamNames = [];
            }

            var isGenericType = genericParamNames.Count != 0;

            var genericParamsText = isGenericType ?
                $"<{string.Join(", ", genericParamNames)}>" :
                string.Empty;

            Code =
            $$"""
            {{accessModifier}} partial struct {{structNameText}}{{genericParamsText}}{{interfacesText}}
            {
                {{body.Code.ToString().IndentTrailing()}}
            }
            """;
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