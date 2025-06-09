using System;

namespace NoParamlessCtor.SourceGenerator.Attributes
{
    [AttributeUsage(AttributeTargets.Struct, AllowMultiple = false)]
    public sealed class NoParamlessCtorAttribute: Attribute;
}