# Platform Portability

Platform portability refers to identifying APIs that are not portable among the various .NET Platforms. These include 
Microsoft supported platforms (Windows, Windows apps, DNX) as well as other implementations, such as Mono and Xamarin.
Some APIs may be removed on certain platforms (such as AppDomains, File I/O, etc), or refactored into other
types (such as some Reflection APIs). Sometimes, the fix is relatively simple, sometimes not as simple. This tool provides
information to help guide a developer to rework or rewrite certain parts of an assembly to be more portable and resilient
to version changes.

## Reports

The reports for the portability analysis contains information about the APIs the tool has flagged as being problematic
when moving to one of the targets requested. The output formats currently supported are:

- JSON
- HTML
- Excel

Each of these have their pros and cons, and may be more suitable for certain usage than others.  Generally, the HTML report 
provides a good overview of the issues in the assemblies being analyzed, while the Excel provides a good place to check off
issues as they are examined. 

**Note:** The number of times an API is found in an assembly has no bearing on the report. In 
fact, this knowledge is not even determined by the tool as it would drastically increase the analysis time. Thus, the presence
of an API indicates the usage of the API, but not how often or where it happens. There is a [work item](https://github.com/Microsoft/dotnet-apiport/issues/103) 
to track adding this functionality, and if this would be useful to you, please join the discussion and share the scenarios for this.

## Example

As an example, consider the following snippet of code:

```csharp
using System;
using System.Runtime.Serialization;

namespace Application
{
    [Serializable]
    public class SomeCustomException : Exception
    {
        public SomeCustomException() { }

        protected SomeCustomException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        { }
    }
}
```

If we run it through the tool with `ApiPort.exe analyze -f foo.dll`, we will get the output:

| Assembly | Target Framework | ASP.NET 5 | Windows | .NET Framework | Windows Phone |
|----------|------------------|-----------|---------|----------------|---------------|
| foo.dll  |.NETFramework,Version=v4.5 | 94.44 | 94.44 | 100 | 94.44 |

This says that it is 100% portable to the .NET Framework which we expect because it is compiled for the .NET Framework. However,
we see that porting it to Windows, Windows Phone, and ASP.NET 5 are all about 94%. If we dive into the details section of the
report, we see where the issues are:

| Assembly | ASP.NET 5 | Windows | .NET Framework | Windows Phone | Recommended changes |
|----------|-----------|---------|----------------|---------------|---------------------|
| foo.dll  | Not supported | Not supported | Supported: 1.1+ | Not supported | Remove serialization constructors on custom Exception types |
| foo.dll  | Not supported | Not supported | Supported: 1.1+ | Not supported | Remove.  Ctor overload taking SerializationInfo is not applicable in new surface area |

There is a column at the end that is called `Recommended changes`.  These are small hints as to what should be done with certain APIs
that have known issues or are no longer supported.  These recommendations are indicating that serialization of exceptions is no long supported,
and implementing them are no longer necessary.  These recommendations are geared for moving forward to the .NET Core based platforms, of which
ASP.NET 5 and Windows are a part.  However, if you want to continue to also target .NET Framework, you'll probably want to continue using
this design as it is supported on that platform.  So, a way to accomplish this would be to use a compiler `#if/def`:

```csharp
using System;
using System.Runtime.Serialization;

namespace Application
{
#if DESKTOP
    [Serializable]
#endif
    public class SomeCustomException : Exception
    {
        public SomeCustomException() { }

#if DESKTOP
        protected SomeCustomException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        { }
#endif
    }
}
```

Now, when you compile for .NET Framework and set the `DESKTOP` compile flag, it will include the serialization, but not when you compile for
ASP.NET 5 or Windows.