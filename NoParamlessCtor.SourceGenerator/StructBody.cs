using System.Text;

namespace NoParamlessCtor.SourceGenerator
{
    public readonly struct StructBody()
    {
        public readonly StringBuilder Code = new();

        public void AppendCode(string code)
        {
            Code.Append(code);
        }
    }
}