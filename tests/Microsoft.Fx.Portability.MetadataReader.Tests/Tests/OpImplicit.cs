namespace Microsoft.Fx.Portability.MetadataReader.Tests
{
    class CallOtherClass_OpImplicit
    {
        static void Main(string[] args)
        {
            Class1_OpImplicit<int> list = new Class2_OpImplicit<int>();
        }
    }

    public class Class1_OpImplicit<T> { }

    internal struct Class2_OpImplicit<T>
    {
        public static implicit operator Class1_OpImplicit<T>(Class2_OpImplicit<T> other)
        {
            return new Class1_OpImplicit<T>();
        }
    }
}