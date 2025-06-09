using System;
using Microsoft.CodeAnalysis;
using NoParamlessCtor.SourceGenerator.Helpers;

namespace NoParamlessCtor.SourceGenerator
{
    public readonly struct StructBlock
    {
        public readonly ITypeSymbol TypeSymbol;

        public readonly string Code;

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

            Code =
            $$"""
            {{accessModifier}} partial struct {{structNameText}}{{interfacesText}}
            {
                {{body.Code.ToString().IndentTrailing()}}
            }
            """;
        }
    }
}