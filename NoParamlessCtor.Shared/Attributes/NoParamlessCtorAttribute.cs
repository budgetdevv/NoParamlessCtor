using System;

namespace NoParamlessCtor.Shared.Attributes
{
    [AttributeUsage(AttributeTargets.Struct, AllowMultiple = false)]
    public sealed class NoParamlessCtorAttribute: Attribute;
}