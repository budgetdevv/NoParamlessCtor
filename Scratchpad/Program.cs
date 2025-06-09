using NoParamlessCtor.Shared.Attributes;

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

    [NoParamlessCtor]
    public ref partial struct RefCtor(ref PrimaryCtor value)
    {
        public ref PrimaryCtor Value = ref value;
    }

    [NoParamlessCtor]
    public ref partial struct InCtor(in PrimaryCtor value)
    {
        public ref readonly PrimaryCtor Value = ref value;
    }

    internal static class Program
    {
        private static void Main(string[] args)
        {
            // Will generate compile error!

            // var nonPrimaryCtor = new NonPrimaryCtor();
            //
            // var primaryCtor = new PrimaryCtor();
            //
            // var refCtor = new RefCtor();
            //
            // var inCtor = new InCtor();

            // Compiles fine with parameterized constructors

            var nonPrimaryCtor = new NonPrimaryCtor("Hello");

            var primaryCtor = new PrimaryCtor("Hello", "World");

            var refCtor = new RefCtor(ref primaryCtor);

            var inCtor = new InCtor(in primaryCtor);
        }
    }
}