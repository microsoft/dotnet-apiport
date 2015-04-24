namespace Microsoft.Fx.Portability.MetadataReader.Tests
{
    class CallOtherClass_OpExplicit
    {
        static void Main(string[] args)
        {
            Class1_OpExplicit<int> list = (Class1_OpExplicit<int>)new Class2_OpExplicit<int>();
        }
    }

    public class Class1_OpExplicit<T> { }

    internal struct Class2_OpExplicit<T>
    {
        public static explicit operator Class1_OpExplicit<T>(Class2_OpExplicit<T> other)
        {
            return new Class1_OpExplicit<T>();
        }
    }
}