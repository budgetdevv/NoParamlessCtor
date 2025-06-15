using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NoParamlessCtor.SourceGenerator.Helpers;

namespace NoParamlessCtor.SourceGenerator.CodeGeneration
{
    public enum TypeBlockKind
    {
        Struct = 1,
        Class = 2,
        Interface = 3,
        Record = 4,
        RecordStruct = 5
    }

    public readonly struct TypeBlock
    {
        public readonly TypeDeclarationSyntax DeclarationSyntax;

        public readonly INamedTypeSymbol TypeSymbol;

        public readonly TypeBlockKind BlockKind;

        public readonly string Code;

        public TypeBlock(
            TypeDeclarationSyntax declarationSyntax,
            INamedTypeSymbol typeSymbol,
            string body)
        {
            DeclarationSyntax = declarationSyntax;

            TypeSymbol = typeSymbol;

            var blockKind = BlockKind = GetBlockKind(declarationSyntax);

            GetTypeMetadata(
                declarationSyntax,
                typeSymbol,
                out var accessModifier,
                out var structNameText,
                out var genericParamNames,
                out var genericParamsText,
                out var isUnsafe
            );

            Code = GenerateCode(
                blockKind,
                declarationSyntax,
                typeSymbol,
                accessModifier,
                isUnsafe,
                structNameText,
                genericParamsText,
                body
            );
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TypeBlockKind GetBlockKind(TypeDeclarationSyntax declarationSyntax)
        {
            TypeBlockKind blockKind;

            switch (declarationSyntax)
            {
                case StructDeclarationSyntax:
                    blockKind = TypeBlockKind.Struct;
                    break;

                case ClassDeclarationSyntax:
                    blockKind = TypeBlockKind.Class;
                    break;

                case InterfaceDeclarationSyntax:
                    blockKind = TypeBlockKind.Interface;
                    break;

                case RecordDeclarationSyntax recordDeclaration:
                    var isRecordStruct = recordDeclaration
                        .ClassOrStructKeyword
                        .IsKind(SyntaxKind.StructKeyword);

                    blockKind = isRecordStruct ?
                        TypeBlockKind.RecordStruct :
                        TypeBlockKind.Record;

                    break;

                default:
                    throw new ArgumentException($"Unsupported type declaration syntax: {declarationSyntax.GetType().Name}");
            }

            return blockKind;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void GetTypeMetadata(
            TypeDeclarationSyntax declarationSyntax,
            INamedTypeSymbol typeSymbol,
            out string accessModifier,
            out string structNameText,
            out IReadOnlyList<string> genericParamNames,
            out string genericParamsText,
            out bool isUnsafe)
        {
            accessModifier = typeSymbol.DeclaredAccessibility.ToText();

            structNameText = typeSymbol.Name;

            var typeParams = typeSymbol.TypeParameters;

            var genericParamNamesList = new List<string>(typeParams.Length);

            genericParamNames = genericParamNamesList;

            foreach (var genericParam in typeParams)
            {
                genericParamNamesList.Add(genericParam.GetFullyQualifiedName());
            }

            var isGenericType = genericParamNamesList.Count != 0;

            genericParamsText = isGenericType ?
                $"<{string.Join(", ", genericParamNamesList)}>" :
                string.Empty;

            isUnsafe = declarationSyntax.ContainsKeyword("unsafe");
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string GenerateCode(
            TypeBlockKind kind,
            TypeDeclarationSyntax declarationSyntax,
            INamedTypeSymbol typeSymbol,
            string accessModifier,
            bool isUnsafe,
            string structNameText,
            string genericParamsText,
            string body)
        {
            var kindText = kind switch
            {
                TypeBlockKind.Struct => "struct",
                TypeBlockKind.Class => "class",
                TypeBlockKind.Interface => "interface",
                TypeBlockKind.Record => "record",
                TypeBlockKind.RecordStruct => "record struct",
                _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, null)
            };

            var unsafeText = isUnsafe ? "unsafe " : string.Empty;

            var code =
            $$"""
            {{accessModifier}} {{unsafeText}}partial {{kindText}} {{structNameText}}{{genericParamsText}}
            {
                {{body.IndentTrailing()}}
            }
            """;

            var parentTypeSymbol = typeSymbol.ContainingType;

            if (parentTypeSymbol == null)
            {
                return code;
            }

            var parentDeclarationSyntax = (declarationSyntax.Parent as TypeDeclarationSyntax)!;

            return new TypeBlock(
                parentDeclarationSyntax,
                parentTypeSymbol,
                code
            ).Code;
        }
    }
}