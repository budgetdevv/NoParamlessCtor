using NoParamlessCtor.SourceGenerator.Attributes;

namespace Scratchpad
{
    [NoParamlessCtor]
    public partial struct NonPrimaryCtor
    {
        public string Text;

        public NonPrimaryCtor(string text)
        {
            Text = text;
        }
    }

    [NoParamlessCtor]
    public partial struct PrimaryCtor(string text1, string text2)
    {
        public string Text1 = text1;

        public string Text2 = text2;
    }

    internal static class Program
    {
        private static void Main(string[] args)
        {
            var nonPrimaryCtor = new NonPrimaryCtor();

            var primaryCtor = new PrimaryCtor();
        }
    }
}