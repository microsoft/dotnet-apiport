# Introduction

This repository holds a collection of tools for analyzing assemblies targeting the .NET Framework. As the .NET Framework 
has grown, there has been a number of pain points in moving between supported platforms and versions.  The goal of these 
tools is to help identify possible problem areas in an assembly and help direct targeted testing, by identifying APIs that:

1. Are not portable to specific platforms
2. Have breaking changes between 4.x versions

## Platform Portability

The first goal of these tools are to help identify APIs that are not portable among the various .NET Platforms. These 
include Microsoft supported platforms (Windows, Windows apps, DNX) as well as other implementations, such as Mono and 
Xamarin.  Some APIs may be removed on certain platforms (such as AppDomains, File I/O, etc), or refactored into other
types (such as some Reflection APIs). Sometimes the fix will be relatively simple while other times it may be more involved. 
This tool provides information to help guide a developer to rework or rewrite certain parts of an assembly to be more portable
 and resilient to version changes. For details please see [here](PlatformPortability.md).

## Breaking Changes between .NET 4.x versions

Another goal of the tools is to provide guidance and insight into possible breaking changes on the .NET Framework that may
apply to a given assembly. This functionality is currently restricted to .NET Framework 4.x given that it is updated in-place
with no side-by-side support for alternative versions.  Most of these are considered benign changes that shouldn't affect
most applications; however, we understand that what may be low impact for one scenario may be a very impactful breaking change
for another. For details please see [here](BreakingChanges.md).

# Usage

The tools are available as a command line tool, a VS plugin, as well as a collection of libraries. The VS plugin is updated at
a slower pace than the command line tool (for instance, it currently does not support the breaking change functionality). The
command line tool is available at [github](http://github.com/microsoft/dotnet-apiport/releases), and requires .NET 4.5 or Mono 
(there is a [work item](https://github.com/Microsoft/dotnet-apiport/issues/117) to convert this to a DNX application).

The tool understands three commands:

### Analyze

Arguably the most important function of the tool is its ability to analyze an assembly. This can take in a file, collection of
files, or a directory of assemblies.  

`ApiPort.exe analyze <options>`

The current options are:

```
  -f, --file=VALUE              [Required] Path to assembly file or directory of
                                assemblies.
  -o, --out=VALUE               Output file name
  -d, --description=VALUE       Description of the submission
  -t, --target=VALUE            The target you want to check against.
  -r, --resultFormat=VALUE      The report output format
  -p, --showNonPortableApis     Calculate non-portable APIs
  -b, --showBreakingChanges     Calculate breaking changes on full .NET Framework
  -u, --showRetargettingIssues  Include the retargetting issues in the reports
      --noDefaultIgnoreFile     Do not use the standard assembly ignore list
                                  when analyzing breaking changes. The default
                                  ignore list can be found at KnownSafeBreaks.json
  -i, --ignoreAssemblyFile=VALUE
                                Specifies a json file defining assemblies that
                                  should not be analyzed for specific targets
                                  while analyzing breaking changes. This can be
                                  useful for excluding assemblies that are known
                                  to not regress on certain .NET Framework
                                  versions due to breaking changes. Note that,
                                  currently, this parameter only affects breaking
                                  change analysis; not portability analysis.
  -s, --suppressBreakingChange=VALUE
                                Specifies a breaking change (by ID) to suppress
                                  during breaking change analysis. Any breaking
                                  changes with IDs specified for suppression will
                                  not be reported.
  -h, -?, --help                Show help
```

For more details on analysis for portability look [here](PlatformPortability.md). For breaking change analysis please
look [here](BreakingChanges.md).

For example, to analyze `foo.dll` against `.NET Core` and the latest `.NET Framework` and get an HTML report, the following command would be run:

```
ApiPort.exe analyze -f foo.dll -t ".NET CORE, Version=5.0" -t ".NET Framework" -r HTML
```

### List targets

The targets available to analyze against are retrieved from a service that is updated regularly. These targets change over time as new
platforms are available. The service will default to the most current platforms and versions.

For example, the command `ApiPort.exe listTargets` will output:

```
Available Targets:
- .NET Core [Version: 5.0*]
- .NET Core (Cross-platform) [Version: 5.0]
- .NET Framework [Version: 1.1; 2.0; 3.0; 3.5; 4.0; 4.5; 4.5.1; 4.5.2; 4.6; 4.6.1*]
- .NET Native [Version: 1.0*]
- ASP.NET 5 [Version: 1.0*]
- Mono [Version: 3.3.0.0]
- Silverlight [Version: 2.0; 3.0; 4.0; 5.0]
- Windows [Version: 8.0; 8.1]
- Windows Phone [Version: 8.1]
- Windows Phone Silverlight [Version: 7.0; 7.1; 8.0; 8.1]
- Xamarin.Android [Version: 4.12.1]
- Xamarin.iOS [Version: 7.2.2]

Available Grouped Targets:
- Mobile (Windows, Windows Phone, Xamarin.Android, Xamarin.iOS)

Notes on usage:
- In order to specify a version, please use the following format with the '-targets' option:
        (Target Name), Version=(Version)

- Versions marked with an asterisk (*) implies that these are default targets if none are submitted.
```

Any combination of these can be supplied to the `analyze` command with the `-t` option.

### List output formats

The output formats available for the results can be retrieved with `ApiPort.exe listOutputFormats`.  This currently
results in the following output formats:

- json
- HTML
- Excel

# Alternate modes

The tool by default will gather the results and submit to a webservice that will analyze the data to determine which APIs need to be addressed. For full
details on this process, please read the [privacy policy](/docs/LicenseTerms/Microsoft%20.NET%20Portability%20Analyzer%20Privacy%20Statement.txt).
There are two alternate modes that can be used to alter this workflow. 

## See the data being transmitted

The first option is to output the request to a file. This will result in an output that shows what data is being transmitted to the service, but provides
no details as to API portability or breaking changes. This is a good option if you would like to see what data will be collected.

In order to enable this mode, create a file `unity.config` and place it in the same directory as `ApiPort.exe`. Add the following contents:

```xml
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <configSections>
    <section name="unity" type="Microsoft.Practices.Unity.Configuration.UnityConfigurationSection, Microsoft.Practices.Unity.Configuration"/>
  </configSections>
  <unity xmlns="http://schemas.microsoft.com/practices/2010/unity">
    <typeAliases>
      <typeAlias alias="singleton" type="Microsoft.Practices.Unity.ContainerControlledLifetimeManager, Microsoft.Practices.Unity" />
      <typeAlias alias="IApiPortService" type="Microsoft.Fx.Portability.IApiPortService, Microsoft.Fx.Portability" />
      <typeAlias alias="FileOutputApiPortService" type="ApiPort.FileOutputApiPortService, ApiPort" />
    </typeAliases>
    <container>
      <register type="IApiPortService" mapTo="FileOutputApiPortService"  >
        <lifetime type="singleton" />
      </register>
	  <instance name="DefaultOutputFormat" value="json" />
    </container>
  </unity>
</configuration>
```

Now, when you run, it will output a file with the information that is sent to the .NET Portability service.

## Run the tool in an offline mode

Another option is to enable full offline access. This mode will not get automatic updates and no official releases of it are available. In order to use this mode,
the solution must be manually built. To do so, please follow these steps:

1. Clone the project: `git clone https://github.com/Microsoft/dotnet-apiport`
2. Build the project: `build.cmd`. 

	*Note: This command must be used as it gathers the correct assemblies for offline mode. Building in VS does not do this.*
	
3. Go to `bin\release\ApiPort.Offline`
4. Run `ApiPort.exe` from this directory as normal.

Additional reports can be generated in offline mode. Any implementation of `Microsoft.Fx.Portability.Reporting.IReportWriter` can be used. Add an entry to `unity.config` 
following the pattern of the HTML and json writers. The offline mode will pick it up and allow reports to be returned in custom formats.
