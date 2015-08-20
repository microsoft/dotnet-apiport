# Introduction

This repository holds a collection of tools for analyzing assemblies targeting the .NET Framework. As the .NET Framework 
has grown, there has been a number of pain points in moving between supported platforms and versions.  The goal of these 
tools is to help identify possible problem areas in an assembly and help direct targeted testing, by identifying APIs that:

1. Are not portable to specific platforms
2. May be problematic when moving to different versions

## Platform Portability

The first goal of these tools are to help identify APIs that are not portable among the various .NET Platforms. These 
include Microsoft supported platforms (Windows, Windows apps, DNX) as well as other implementations, such as Mono and 
Xamarin.  Some APIs may be removed on certain platforms (such as AppDomains, File I/O, etc), or refactored into other
types (such as some Reflection APIs). Sometimes, the fix is relatively simple, sometimes not as simple. This tool provides
information to help guide a developer to rework or rewrite certain parts of an assembly to be more portable and resilient
to version changes. For details, please see [here](PlatformPortability.md).

## Problematic APIs

Another goal of the tool is to provide guidance and insight into possible breaking changes on the .NET Framework that may
apply to a given assembly. This functionality is currently restricted to .NET Framework 4.x given that it is updated in-place
with no side-by-side support for alternative versions.  Most of these are considered benign changes that shouldn't affect
most applications. However, we understand that what may be low impact for one scenario may be very impactful breaking change
for another. For details, please see [here](BreakingChanges.md).

# Usage

The tools are available as a command line tool, a VS plugin, as well as a collection of libraries. The VS plugin is updated at
a slower pace than the command line tool (for instance, it currently does not support the breaking change functionality). The
command line tool is available at [github](http://github.com/microsoft/dotnet-apiport/releases), and requires .NET 4.6 or Mono 
(there is a [work item](https://github.com/Microsoft/dotnet-apiport/issues/117) to convert this to a DNX application).

The tool understands three commands:

### Analyze

Arguably the most important function of the tool is its ability to analyze an assembly. This can take in a file, collection of
files, or a directory of assemblies.  

`ApiPort.exe analyze <options>`

The current options are:

```
  -f, --file=VALUE           [Required] Path to assembly file or directory of
                               assemblies.
  -o, --out=VALUE            Output file name
  -d, --description=VALUE    Description of the submission
  -t, --target=VALUE         The target you want to check against.
  -r, --resultFormat=VALUE   The report output format
  -p, --showNonPortableApis  Calculate non-portable APIs
  -b, --showBreakingChanges  Calculate breaking changes on full .NET Framework
      --noDefaultIgnoreFile  Do not use the standard assembly ignore list
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
  -h, -?, --help             Show help
```

For more details on analysis for portability, look [here](PlatformPortability.md) and for breaking change analysis, please
look [here](BreakingChanges.md).

For example, to analyze `foo.dll` against `.NET Core` to get an HTML report, the following command would be run:

```
ApiPort.exe analyze -f foo.dll -t ".NET CORE, Version=5.0 -r HTML"
```

### List targets

The targets available to analyze against are retrieved from a service that is updated regularly (except when running in
[offline mode](OfflineMode.md)).  These are ever growing, and include the previous versions of the framework, although
it defaults to the most current platforms and versions.

For example, the command `ApiPort.exe listTargets` will output:

```
Available Targets:
- .NET Core [Version: 5.0*]
- .NET Framework [Version: 1.1; 2.0; 3.0; 3.5; 4.0; 4.5; 4.5.1; 4.5.2; 4.6*]
- .NET Native [Version: 1.0*]
- ASP.NET 5 [Version: 1.0*]
- Mono [Version: 4.5]
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