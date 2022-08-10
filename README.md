# .NET API Portability

> ***Note:** We're in the process of deprecating API Port in favor of integrating binary analysis directly into [.NET Upgrade Assistant](https://github.com/dotnet/upgrade-assistant). In the upcoming months, we're going to shutdown the backend service of API Port which will require to use the tool in offline mode. The instructions to use API Port in offline mode can be found [here](docs/Console/README.md#run-the-tool-in-an-offline-mode).*

This repository contains the source code for .NET Portability Analyzer tools and
dependencies.

|Branch|Build Status
|---|---
|master|[![Build Status](https://devdiv.visualstudio.com/DevDiv/_apis/build/status/CoreFxTools/dotnet-apiport-yaml?branchName=master)](https://devdiv.visualstudio.com/DevDiv/_build/latest?definitionId=12912&branchName=master)
|dev|[![Build Status](https://devdiv.visualstudio.com/DevDiv/_apis/build/status/CoreFxTools/dotnet-apiport-yaml?branchName=dev)](https://devdiv.visualstudio.com/DevDiv/_build/latest?definitionId=12912&branchName=dev)

For a quick introduction, check out [this video on Shows][Shows Video]:

[<img src="https://sec.ch9.ms/ch9/031c/f3d7672b-dd71-4a18-a8b4-37573c08031c/DotNetPortabilityAnalyzer_960.jpg" width="480" />][Shows Video]

There is a Visual Studio extension available for VS 2017 and VS 2019: [.NET Portability Analyzer](https://marketplace.visualstudio.com/items?itemName=ConnieYau.NETPortabilityAnalyzer)

## Using this Repository

See our [contributing guide](CONTRIBUTING.md) for instructions to
build and run from the source code in this repo.

Sample usage to run the analysis from the command line:

```ps1
./init.ps1
dotnet build src/ApiPort/ApiPort/ApiPort.csproj
dotnet bin/Debug/ApiPort/netcoreapp2.1/ApiPort.dll -- listTargets
dotnet bin/Debug/ApiPort/netcoreapp2.1/ApiPort.dll -- analyze -f Foo.dll -r HTML
```

If using bash for your shell, for convenience you may create an alias command adding the following to your `~/.bash_profile`. Replace `{dotnet-apiport-folder}` with the path where you cloned the repo.

```bash
alias apiport="dotnet {dotnet-apiport-folder}/bin/Debug/ApiPort/netcoreapp2.1/ApiPort.dll"
```

This will allow you to use apiport globally from the command line: `apiport analyze -f Foo.dll -r HTML`

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

### Downloads

|     | Location |
| :--- | :--- |
| ApiPort CLI | [ApiPort Download][ApiPort Download] |
| Visual Studio Extension |  [Open VSIX Gallery][VSIX Gallery] |


### Privacy:
We only send .NET APIs and its caller user assembly names to the service to analyze for portability and generate report. For more information, check out our [privacy policy](https://privacy.microsoft.com/en-us/privacystatement).

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

[Shows Video]: https://docs.microsoft.com/shows/seth-juarez/brief-look-net-portability-analyzer
[Contributing Guide]: https://github.com/dotnet/corefx/wiki/Contributing
[Developer Guide]: https://github.com/dotnet/corefx/wiki/Developer-Guide
[DocId]: https://msdn.microsoft.com/en-us/library/fsbx0t7x.aspx
[Issues-Open]: https://github.com/Microsoft/dotnet-apiport/issues?q=is%3Aopen+is%3Aissue
[PR]: https://github.com/Microsoft/dotnet-apiport/pulls
[PR-Closed]: https://github.com/Microsoft/dotnet-apiport/pulls?q=is%3Apr+is%3Aclosed
[PR-Open]: https://github.com/Microsoft/dotnet-apiport/pulls?q=is%3Aopen+is%3Apr
[ApiPort Download]: https://aka.ms/apiportdownload
[System.Reflection.Metadata]: https://github.com/dotnet/corefx/tree/master/src/System.Reflection.Metadata
[VSIX Gallery]: http://vsixgallery.com/extension/55d15546-28ca-40dc-af23-dfa503e9c5fe
