namespace Microsoft.Fx.Portability.MetadataReader.Tests.Tests
{
    class CallOtherClass
    {
        static void Main(string[] args)
        {
            GenericClass<int>.MemberWithDifferentGeneric("hello");
        }
    }

    public class GenericClass<TResult>
    {
        internal static TResult MemberWithDifferentGeneric<TAntecedentResult>(TAntecedentResult result)
        {
            return default(TResult);
        }
    }
}
