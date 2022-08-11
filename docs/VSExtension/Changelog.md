# Changelog

There are changes to each version of the .NET Portability Analyzer that has been released on [Visual Studio extension gallery](https://marketplace.visualstudio.com/items?itemName=ConnieYau.NETPortabilityAnalyzer)

## 2.8.0

__August 10, 2022__

* Server-side analysis disabled 
    * Due to a regression in our legacy backend service, this and any future versions will work only in Offline mode
    * HTML output disabled (Visual Studio limitation which server-side analysis worked around)
    * Large projects may experience memory exhaustion, especially on 32-bit machines, but we did not see this in our testing
* Related, take note of [the deprecation announcement](../../README.md) on the main page of this repo

## 2.5.0

__May 30, 2018__

* Accessibility improvements
    * Improve screen narrator support
    * Improve experience when in high contrast mode
    * Fix tab ordering
    * Fix accessibility of HTML reports
* Add support for proxies when contacting service
* Remove support for Visual Studio 2015

## 1.4.0

__May 12, 2017__

* Support for Visual Studio 2015 and Visual Studio 2017
* Allow solution analysis even if there are non .NET projects in the solution.  Only analysis on the .NET projects is performed
* Various bug fixes

## 1.3.0

__October 10, 2016__

* Multiple output formats supported
* Can analyze an entire solution
* Adds options as to default output and file name
* Tool window shows list of reports that can be saved
* Many performance improvements
* Bug fixes for persisting Analyzer options
* Updating analysis endpoint