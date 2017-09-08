using System.Runtime.Versioning;

namespace Microsoft.Fx.Portability.Tests.TestData
{
    public static class TestFrameworks
    {
        internal static readonly FrameworkName Windows80 = new FrameworkName("Windows,Version=v8.0");
        internal static readonly FrameworkName Windows81 = new FrameworkName("Windows,Version=v8.1");
        internal static readonly FrameworkName NetCore50 = new FrameworkName(".NET Core,Version=v5.0");
        internal static readonly FrameworkName Net11 = new FrameworkName(".NET Framework,Version=v1.1");
        internal static readonly FrameworkName Net40 = new FrameworkName(".NET Framework,Version=v4.0");
        internal static readonly FrameworkName NetStandard16 = new FrameworkName(".NETStandard,Version=v1.6");
        internal static readonly FrameworkName NetStandard20 = new FrameworkName(".NETStandard,Version=v2.0");
    }
}
