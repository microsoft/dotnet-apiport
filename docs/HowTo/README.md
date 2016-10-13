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

## .NET Portability Analyzer Tools

* [Console Application (.NET Framework)](../Console)
* [.NET Core Application](../Console/README.md#using-net-core-application)
* [Visual Studio 2015 Extension](../VSExtension)