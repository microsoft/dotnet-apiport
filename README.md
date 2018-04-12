# .NET API Portability

This repository contains the source code for .NET Portability Analyzer tools and
dependencies.

|Branch|Build Status|
|---|---|
|master|[![][BuildStatus-Master]][myget]|
|dev|[![][BuildStatus-Dev]][myget]|

For a quick introduction, check out [this video on Channel 9][Channel 9 Video]:

[<img src="https://sec.ch9.ms/ch9/031c/f3d7672b-dd71-4a18-a8b4-37573c08031c/DotNetPortabilityAnalyzer_960.jpg" width="480" />][Channel 9 Video]

## Using this Repository

### Windows
There is a Visual Studio extension available for VS2017 and VS2015: [.NET Portability Analyzer](https://marketplace.visualstudio.com/items?itemName=ConnieYau.NETPortabilityAnalyzer)

Download and build for yourself:
1. Install [Visual Studio 2017 with .NET Core Workload][Visual Studio 2017]
2. Building:
   * Visual Studio: `PortabilityTools.sln`
   * Powershell: `.\build.ps1 -Configuration Debug -Platform AnyCPU`

### Linux/Mac

1. Install [.NET Core SDK](https://www.microsoft.com/net/core)
2. Execute: `build.sh`
3. Go to: `bin/Debug/ApiPort/netcoreapp1.0`
4. Run ApiPort by executing: `dotnet ApiPort.dll`
  * Example: `dotnet ApiPort.dll listTargets`
  * Example: `dotnet ApiPort.dll analyze -f Foo.dll -r HTML`
5. For convenience, create an alias command adding the following to your `~/.bash_profile`. Replace `{dotnet-apiport-folder}` with the path where you cloned the repo.
```
alias apiport="dotnet {dotnet-apiport-folder}/bin/Debug/ApiPort/netcoreapp2.0/ApiPort.dll"
```
This will alow you to use apiport globally from the command line: `apiport analyze -f Foo.dll -r HTML`

## Documentation

* [Introduction](docs/HowTo)
* [Platform Portability](docs/HowTo/PlatformPortability.md)
* [Breaking Changes](docs/HowTo/BreakingChanges.md)
* [.NET Portability Analyzer (Console application)](docs/Console)
    * [.NET Core application](docs/Console/README.md#using-net-core-application)
* [.NET Portability Analyzer (Visual Studio extension)](docs/VSExtension)

## Projects

| Project | Description |
| :------ | :---------- |
| ApiPort | Cross-platform console tool to access portability service |
| ApiPort.Vsix | Visual Studio Extension |
| Microsoft.Fx.Portability | Provides common types for API Port |
| Microsoft.Fx.Portability.MetadataReader | Implements a dependency finder based off of [System.Reflection.Metadata][System.Reflection.Metadata]. The library will generate DocIds that conform to [these specifications][DocId]. |
| Microsoft.Fx.Portability.Offline | Provides access to data in an offline setting so network calls are not needed |
| Microsoft.Fx.Portability.Reporting.Excel | Provides support for an Excel spreadsheet report for ApiPort |
| Microsoft.Fx.Portability.Reporting.Html | Provides support for an HTML report for ApiPort |
| Microsoft.Fx.Portability.Reporting.Json | Provides support for a JSON reporter for ApiPort |

### Builds

|     | Location |
| :--- | :--- |
| Libraries | [MyGet][myget] |
| Visual Studio Extension |  [Open VSIX Gallery][VSIX Gallery] |

## How to Engage, Contribute and Provide Feedback

Here are some ways to contribute:
* [Update/Add recommended changes](docs/RecommendedChanges)
* Try things out!
* File issues
* Join in design conversations

Want to get more familiar with what's going on in the code?
* [Pull requests][PR]: [Open][PR-Open]/[Closed][PR-Closed]

Looking for something to work on? The list of [up-for-grabs issues][Issues-Open]
is a great place to start.

* [How to Contribute][Contributing Guide]
    * [Contributing Guide][Contributing Guide]
    * [Developer Guide][Developer Guide]

## Related Projects

For an overview of all the .NET related projects, have a look at the
[.NET home repository](https://github.com/Microsoft/dotnet).

## License

This project is licensed under the [MIT license](LICENSE).

[BuildStatus-Master]: https://devdiv.visualstudio.com/_apis/public/build/definitions/0bdbc590-a062-4c3f-b0f6-9383f67865ee/484/badge
[BuildStatus-Dev]: https://devdiv.visualstudio.com/_apis/public/build/definitions/0bdbc590-a062-4c3f-b0f6-9383f67865ee/7913/badge
[Channel 9 Video]: https://channel9.msdn.com/Blogs/Seth-Juarez/A-Brief-Look-at-the-NET-Portability-Analyzer
[Contributing Guide]: https://github.com/dotnet/corefx/wiki/Contributing
[Developer Guide]: https://github.com/dotnet/corefx/wiki/Developer-Guide
[DocId]: https://msdn.microsoft.com/en-us/library/fsbx0t7x.aspx
[Issues-Open]: https://github.com/Microsoft/dotnet-apiport/issues?q=is%3Aopen+is%3Aissue
[PR]: https://github.com/Microsoft/dotnet-apiport/pulls
[PR-Closed]: https://github.com/Microsoft/dotnet-apiport/pulls?q=is%3Apr+is%3Aclosed
[PR-Open]: https://github.com/Microsoft/dotnet-apiport/pulls?q=is%3Aopen+is%3Apr
[myget]: https://dotnet.myget.org/gallery/dotnet-apiport
[System.Reflection.Metadata]: https://github.com/dotnet/corefx/tree/master/src/System.Reflection.Metadata
[Visual Studio 2017]: https://www.microsoft.com/net/core#windowsvs2017
[VSIX Gallery]: http://vsixgallery.com/extension/55d15546-28ca-40dc-af23-dfa503e9c5fe
