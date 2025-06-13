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

    [NoParamlessCtor]
    public ref partial struct GenericCtor<T1, T2>(ref int t1, in int t2)
    {
        public ref readonly int Text1 = ref t1;

        public ref readonly int Text2 = ref t2;
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
            //
            // var genericCtor = new GenericCtor<int, int>();

            // Compiles fine with parameterized constructors

            var nonPrimaryCtor = new NonPrimaryCtor("Hello");

            var primaryCtor = new PrimaryCtor("Hello", "World");

            var refCtor = new RefCtor(ref primaryCtor);

            var inCtor = new InCtor(in primaryCtor);

            var t1 = 1;

            var t2 = 2;

            var genericCtor = new GenericCtor<int, int>(ref t1, in t2);
        }
    }
}