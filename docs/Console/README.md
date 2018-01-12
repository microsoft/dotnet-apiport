# .NET Portability Analyzer (Console application)

The console tool helps you determine how flexible your application.  The tool
understands the following commands:

| Command | Description | Example |
| :--- | :--- | :--- |
| analyze | Analyzes the portability of an application | [Examples](#apiportexe-analyze-scenarios) |
| listTargets | Lists .NET platforms available for analysis | `ApiPort.exe listTargets` |
| listOutputFormats | Lists available report output formats |`ApiPort.exe listOutputFormats` |
| docIdSearch | Searches for matching docIds | `ApiPort.exe docIdSearch <options>` |
| dump | Outputs the analysis request that will be sent to the service | [Example](#see-the-data-being-transmitted) |

## `ApiPort.exe analyze` Scenarios

Arguably the most important function of the tool is its ability to analyze an
assembly. This can take in a file, collection of files, or a directory of
assemblies.

**Analyzing a file against specific targets and outputting an HTML report**

```
ApiPort.exe analyze -f Foo.dll -t ".NET Framework, Version=4.6.2" -t
".NET Standard, Version=1.6" -r HTML -o AnalysisReport.html
```

The `-f` flag followed by a path represents the file or directory that the
analysis should be performed on; in this case, it is `Foo.dll`.  The multiple
uses of `-t` followed by target names tells the tool what .NET platforms we want
to analyze the input assembly(ies) against. `-r` is the output format of the
report. `-o` is the output name for that report.

So, our analysis will be performed on `Foo.dll` against
`.NET Framework, Version=4.6.2` and `.NET Standard, Version=1.6` and output as
an HTML file, `AnalysisReport.html`.

**Analyzing a directory against the default targets and outputting default
report format**

```
ApiPort.exe analyze -f C:\git\Application\bin\Debug
```

This will analyze all
assemblies that exist under `C:\git\Application\bin\Debug`
recursively, and analyzes those assemblies against the default .NET platforms.
(**Note:** The default platforms can be obtained by running `ApiPort.exe
listTargets` and looking for targets that have an (\*) in them.)

**Analyzing a directory and show breaking changes**

```
ApiPort.exe analyze -f C:\git\Application\bin\Debug -b
```

The `-b` flag will show any APIs that may have different behavior between
versions of .NET Framework due to breaking changes that have been made.  The
entire list of breaking changes in .NET Framework can be found by examining
[Application Compatibility in the .NET Framework](https://docs.microsoft.com/en-us/dotnet/framework/migration-guide/application-compatibility).
For the list of breaking changes we analyze against, look [here](../HowTo/BreakingChanges.md).

**Analyzing a directory and show any non-portable APIs**

```
ApiPort.exe analyze -f C:\git\Application\bin\Debug -p
```

The `-p` flag will highlight any APIs that are not portable against the default
target .NET platforms. (No explicit `-t` arguments were specified, so we use the
default targets.)

## Using .NET Core application

The portability analyzer has a version that targets .NET Core. Possible reasons
for using the .NET Core application include:

* Working on machine without .NET Framework 4.6 installed
* Working on a non-Windows OS

### Compiling, Debugging and Running

**From Commandline**

1. Execute `build.cmd`
2. Execute `dotnet.exe .\bin\Release\ApiPort\netcoreapp1.0\ApiPort.dll [Any other arguments to ApiPort]`

**In Visual Studio 2017**

1. Compile solution
2. Set `ApiPort` as Start-up Project
3. Start debugging (F5)

## Alternate modes

The tool by default will gather the results and submit to a webservice that will
analyze the data to determine which APIs need to be addressed. There are
two alternate modes that can be used to alter this workflow.

### See the data being transmitted

```
ApiPort.exe dump -f [path to file or directory]
```

The `dump` command will output the data that would otherwise be sent to the service.
This is a good option if you would like to see what data will be collected. A file,
which by default is `ApiPortAnalysis.json`, will be created with the information that
would sent to the .NET Portability service.

### Run the tool in an offline mode

Another option is to enable full offline access. This mode will not get
automatic updates and no official releases of it are available. In order to use
this mode, the solution must be manually built. To do so, please follow these
steps:

1. Clone the project: `git clone https://github.com/Microsoft/dotnet-apiport`
2. Compile using the `build.cmd` or `build.sh` script as normal
3. If in Visual Studio, set the project `ApiPort.Offline` as the startup project, or if from command line, go to `bin\[Configuration]\ApiPort.Offline\net46\win7-x64` and run `ApiPort.exe` from this directory.

There is a .NET Core build of the offline mode, but HTML reports will not be generated when running on .NET Core. Other than that, the functionality is expected to be the same.

### Proxies

If you have issues with proxies, please add your proxy information to the catalog script:

1. Edit [`.\init.ps1`](../../init.ps1)
2. Replace the line with `Invoke-WebRequest` with:
  - If your proxy uses your default credentials: `Invoke-WebRequest $url -OutFile $OutputPath -Proxy [Proxy Address] -ProxyUseDefaultCredentials`
  - Otherwise: `Invoke-WebRequest $url -OutFile $OutputPath -Proxy [Proxy Address] -ProxyCredential [Credentials]`
3. Run `build.cmd`

Additional reports can be generated in offline mode. Any implementation of `Microsoft.Fx.Portability.Reporting.IReportWriter` can be
used. An assembly with the name of `Microsoft.Fx.Portability.Writers.[NAME].dll` will be searched for any available implementations.

[Breaking Changes]: https://msdn.microsoft.com/en-US/library/dn458358(v=vs.110).aspx
[Issue #2311]: https://github.com/dotnet/cli/issues/2311
[Privacy Policy]:/docs/LicenseTerms/Microsoft%20.NET%20Portability%20Analyzer%20Privacy%20Statement.txt
[VS2015 C++ Redistributable]: https://www.microsoft.com/en-us/download/details.aspx?id=53587
