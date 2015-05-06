// This is necessary to work around a bug in DNU where it does not stamp the TFM into it.
// Please see https://github.com/aspnet/dnx/issues/1802

#if NET45
[assembly: global::System.Runtime.Versioning.TargetFrameworkAttribute(".NETFramework,Version=v4.5", FrameworkDisplayName = ".NET Framework 4.5")]
#elif DNX451
[assembly: global::System.Runtime.Versioning.TargetFrameworkAttribute("DNX,Version=v4.5.1", FrameworkDisplayName = "DNX 4.5.1")]
#elif DNXCORE50
[assembly: global::System.Runtime.Versioning.TargetFrameworkAttribute("DNXCORE,Version=v5.0", FrameworkDisplayName = "DNXCORE 4.5.1")]
#endif
