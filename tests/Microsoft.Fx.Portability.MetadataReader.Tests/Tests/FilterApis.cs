using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Foo
{
    public class FilterApis
    {
        public static void Test()
        {
            Uri.TryCreate("randomUri", UriKind.Relative, out Uri testResult);

            Console.WriteLine(Microsoft.Bar.Test<int>.Get());
            Console.WriteLine(Other.Test<int>.Get());
        }
    }
}

namespace Other
{
    public static class Test<T>
    {
        public static string Get()
        {
            return typeof(T).ToString();
        }
    }
}

namespace Microsoft.Bar
{
    public static class Test<T>
    {
        public static string Get()
        {
            return typeof(T).ToString();
        }
    }
}
