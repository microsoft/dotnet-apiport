# .NET API Portability

This repository contains the source code for .NET Portability Analyzer tools
and dependencies. This is a work in progress, and does not currently contain
all of the components that we plan on open sourcing. Make sure to watch this
repository in order to be notified as we make changes to and expand it.

## Usage

To use this tool, please refer to the [documentation](docs/HowTo/Introduction.md). For a quick introduction, check out [this video on Channel 9](https://channel9.msdn.com/Blogs/Seth-Juarez/A-Brief-Look-at-the-NET-Portability-Analyzer):

[![A Brief Look at the .NET Portability Analyzer](https://sec.ch9.ms/ch9/031c/f3d7672b-dd71-4a18-a8b4-37573c08031c/DotNetPortabilityAnalyzer_960.jpg)](https://channel9.msdn.com/Blogs/Seth-Juarez/A-Brief-Look-at-the-NET-Portability-Analyzer)

## Projects

Today, the repository contains the following components:

### Tools

| Project | Description |
| :------- | :----------- |
| ApiPort | Console tool to access portability webservice | 

#### A Note About Expected Errors
The ApiPort project contains two csproj files - one for building against the [desktop .NET Framework 4.5](src/ApiPort/ApiPort.csproj), the other for building against [.NET Core](src/ApiPort/ApiPort.Core.csproj). Building a .NET Core executable is still not a well-supported scenario and, as a result, building the project in Visual Studio with NuGet package restore enabled (as it is, by default) will result in errors like the following -
![Errors](docs/DocImages/FalseErrors.png)

**These errors are currently expected and do not keep the project from building successfully.** Notice in the picture that the build succeeded despite the errors.

In order to suppress these false errors, disable Visual Studio's built-in package restore functionality (as described [here](https://github.com/dotnet/corefx/blob/master/Documentation/project-docs/getting-started-core.md#advanced-updating-and-using-nugetexe)). The packages can be restored by building or explicitly restoring packages from the command line (`.tools\nuget.exe src\ApiPort\ApiPort.Core.project.json`).
 Restoring or building from the command line is expected to succeed without any errors.

### Libraries

| Project | Description |
| :------- | :----------- |
| Microsoft.Fx.Portability [![version](https://img.shields.io/myget/dotnet-apiport/v/Microsoft.Fx.Portability.svg)](https://www.myget.org/gallery/dotnet-apiport) | Provides common types for API Port |
| Microsoft.Fx.Portability.MetadataReader [![version](https://img.shields.io/myget/dotnet-apiport/v/Microsoft.Fx.Portability.MetadataReader.svg)](https://www.myget.org/gallery/dotnet-apiport) | Implements a dependency finder based off of [System.Reflection.Metadata](https://github.com/dotnet/corefx/tree/master/src/System.Reflection.Metadata). The library will generate DocIds that conform to [these specifications](https://msdn.microsoft.com/en-us/library/fsbx0t7x.aspx). |
| Microsoft.Fx.Portability.Offline [![version](https://img.shields.io/myget/dotnet-apiport/v/Microsoft.Fx.Portability.Offline.svg)](https://www.myget.org/gallery/dotnet-apiport) | Provides access to data in an offline setting so network calls are not needed |
| Microsoft.Fx.Portability.Reporting.Html [![version](https://img.shields.io/myget/dotnet-apiport/v/Microsoft.Fx.Portability.Reports.Html.svg)](https://www.myget.org/gallery/dotnet-apiport) | Provides an HTML report for ApiPort (used in offline mode) |
| Microsoft.Fx.Portability.Reporting.Json [![version](https://img.shields.io/myget/dotnet-apiport/v/Microsoft.Fx.Portability.Reports.Json.svg)](https://www.myget.org/gallery/dotnet-apiport) | Provides a JSON reporter for ApiPort (used in offline mode) |

More projects are coming soon. Stay tuned!

## Using this Repository

* **Required** Install [Visual Studio 2015](http://www.visualstudio.com/en-us/downloads/visual-studio-2015-downloads-vs.aspx)
* **Required** Windows 10 Tools 1.1 (install via the Visual Studio 2015 installer). The [new style PCL format](http://blog.nuget.org/20150729/Introducing-nuget-uwp.html) will not work without this. [v3.2 of NuGet](https://docs.nuget.org/release-notes/nuget-3.2) is required.

## How to Engage, Contribute and Provide Feedback

Here are some ways to contribute:
* [Update/Add recommended changes](docs/RecommendedChanges/README.md)
* Try things out!
* File issues
* Join in design conversations

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
