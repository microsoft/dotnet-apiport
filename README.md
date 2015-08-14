# .NET API Portability

This repository contains the source code for .NET Portability Analyzer tools
and dependencies. This is a work in progress, and does not currently contain
all of the components that we plan on open sourcing. Make sure to watch this
repository in order to be notified as we make changes to and expand it.

## Projects

Today, the repository contains the following components:

### Tools

| Project | Description |
| ------- | ----------- |
| ApiPort | Console tool to access portability webservice | 

### Libraries

| Project | Description |
| ------- | ----------- |
| Microsoft.Fx.Portability [![version](https://img.shields.io/myget/dotnet-apiport/v/Microsoft.Fx.Portability.svg)](https://www.myget.org/gallery/dotnet-apiport) | Provides common types for API Port |
| Microsoft.Fx.Portability.MetadataReader [![version](https://img.shields.io/myget/dotnet-apiport/v/Microsoft.Fx.Portability.MetadataReader.svg)](https://www.myget.org/gallery/dotnet-apiport) | Implements a dependency finder based off of [System.Reflection.Metadata](https://github.com/dotnet/corefx/tree/master/src/System.Reflection.Metadata). The library will generate DocIds that conform to [these specifications](https://msdn.microsoft.com/en-us/library/fsbx0t7x.aspx). |
| Microsoft.Fx.Portability.Offline [![version](https://img.shields.io/myget/dotnet-apiport/v/Microsoft.Fx.Portability.Offline.svg)](https://www.myget.org/gallery/dotnet-apiport) | Provides access to data in an offline setting so network calls are not needed |
| Microsoft.Fx.Portability.Reporting.Html [![version](https://img.shields.io/myget/dotnet-apiport/v/Microsoft.Fx.Portability.Reports.Html.svg)](https://www.myget.org/gallery/dotnet-apiport) | Provides an HTML report for ApiPort (used in offline mode) |
| Microsoft.Fx.Portability.Reporting.Json [![version](https://img.shields.io/myget/dotnet-apiport/v/Microsoft.Fx.Portability.Reports.Json.svg)](https://www.myget.org/gallery/dotnet-apiport) | Provides a JSON reporter for ApiPort (used in offline mode) |

More projects are coming soon. Stay tuned!

## Using this Repository

* **Required** Install [Visual Studio 2015](http://www.visualstudio.com/en-us/downloads/visual-studio-2015-downloads-vs.aspx)
* **Required** Windows 10 Tools (install via the Visual Studio 2015 installer). The [new style PCL format](http://blog.nuget.org/20150729/Introducing-nuget-uwp.html) will not work without this

## How to Engage, Contribute and Provide Feedback

Some of the best ways to contribute are to try things out, file bugs, and join in design conversations.

Want to get more familiar with what's going on in the code?

* [Pull requests](https://github.com/Microsoft/dotnet-apiport/pulls): [Open](https://github.com/Microsoft/dotnet-apiport/pulls?q=is%3Aopen+is%3Apr)/[Closed](https://github.com/Microsoft/dotnet-apiport/pulls?q=is%3Apr+is%3Aclosed)

Looking for something to work on? The list of [up-for-grabs issues](https://github.com/Microsoft/dotnet-apiport/issues?q=is%3Aopen+is%3Aissue) is a great place to start.

We're re-using the same contributing approach as .NET Core. You can check out the .NET Core [contributing guide][Contributing Guide] at the corefx repo wiki for more details.

* [How to Contribute][Contributing Guide]
    * [Contributing Guide][Contributing Guide]
    * [Developer Guide]

You are also encouraged to start a discussion on the .NET Foundation forums!

[Contributing Guide]: https://github.com/dotnet/corefx/wiki/Contributing
[Developer Guide]: https://github.com/dotnet/corefx/wiki/Developer-Guide

## Related Projects

For an overview of all the .NET related projects, have a look at the
[.NET home repository](https://github.com/Microsoft/dotnet).

## License

This project is licensed under the [MIT license](LICENSE).
