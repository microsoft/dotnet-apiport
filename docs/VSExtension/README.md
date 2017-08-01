# .NET Portability Analyzer (Visual Studio Extension)

The .NET Portability Analyzer helps you determine how flexible your application
is across .NET Platforms. For more information about platform portability
analysis, read ["Platform Portability"][PlatformPortability].

__Features__

* Analysis of a single project, solution, or binaries
* Analysis against multiple .NET platforms simultaneously
* Generation of multiple analysis reports in different formats. Supported 
formats are:
    * JSON
    * HTML
    * Excel
* Configurable report options in the Options pane
* View and save generated reports in a tool window
* Up-to-date server-side analysis
    * __NOTE__ .NET platform targets and report formats are constantly fetched
    from an Azure cloud service that is updated constantly. In addition, the
    analysis is performed remotely so the information is always the latest.
    * We only send .NET APIs to the service to analyze for portability. For more
    information, our privacy policy is [here][PrivacyPolicy].

![Sample report][SampleReport]

## How to obtain

Official releases can be obtained from the [Visual Studio extension gallery][VSGallery]

## Usage

### Analyze a Visual Studio solution or project

You can run the Portability Analyzer over a Visual Studio solution or project by
performing the following:

1. Open a project or solution in Visual Studio
2. Open Solution Explorer
3. Right-click on the project or solution you want to analyze
4. Select "Analyze Assembly Portability"

![Context menu in Solution Explorer][SolutionExplorer-ContextMenu]

After the analysis is complete, if there are any APIs that are not supported on
the selected platforms, check the "Output" window.  There should be some
informational messages containing the API that is not supported and the line 
mapping in source code.  If you double-click on that message, it will take you
to that location in source.

![Source code mapping in Output window][SourceCodeMapping]

### Analyze a compiled binary

You can perform a portability analysis over compiled binaries (.dll or .exe)
files.

1. Click the "Analyze" menu item in the main toolbar
2. Select "Analyze Assembly Portability"
3. Choose binaries to analyze

![Binary analysis menu item][BinaryAnalysis]

### View/Save generated reports

You can view or save previously generated reports in your Visual Studio session
by performing the following:

1. Click the "Analyze" menu item in the main toolbar
2. Select "View analysis reports"
3. A tool window should pop up with all your reports

!["Portability Analysis Results" tool window][ReportToolWindow]

## Configuration

The .NET Portability Analyzer can be configured through the Options window.
Here, you can choose what .NET platforms to perform your analysis against, and
the file name, location and formats of the analysis report.

You can access the settings page via one of three methods:

1. In Solution Explorer
    * Right-click on a project or solution
    * Select "Portability Analyzer Settings"
2. Under "Analyze" in main toolbar
    * Select "Portability Analyzer Settings"
3. Under "Tools" in main toolbar
    * Select "Options..."
    * Go to the ".NET Portability Analyzer" node

![.NET Portability Analyzer options][OptionsPane]

# Changelog

A list of changes can be found in the [Changelog](Changelog.md).

[BinaryAnalysis]: img/analyzeToolbar.png
[OptionsPane]: img/optionsPanel.png
[PlatformPortability]: ../HowTo/PlatformPortability.md
[PrivacyPolicy]: ../LicenseTerms/Microsoft%20.NET%20Portability%20Analyzer%20Privacy%20Statement.txt
[ReportToolWindow]: img/report.toolWindow.png
[SampleReport]: img/analysisReport.png
[SolutionExplorer-ContextMenu]: img/analysisContextMenu.png
[SourceCodeMapping]: img/sourceMapping.output.png
[VSGallery]: https://visualstudiogallery.msdn.microsoft.com/1177943e-cfb7-4822-a8a6-e56c7905292b