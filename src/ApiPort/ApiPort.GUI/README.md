## Portability Analyzer 

The Portability Analyzer is an open source standalone tool that simplifies your porting experience by identifying APIs that are not portable to .NET Core. We discovered that developers are struggling to port their desktop applications from .NET Framework to .NET Core, therefore in order to meet customer needs we updated the .NET Portability Analyzer. The Portability Analyzer is an open source tools that simplifies your porting experience by identifying APIs that are not portable among the various .NET Platforms. 

Running the tool can do three things: 

1.	Generate a spreadsheet within the UI that will report the level of compatibility that your project has with .NET Core 3.0, including the specific APIs that are currently unsupported. This spreadsheet will also contain information regarding the compatibility of NuGet package updates.
2.	If chosen, it will generate an export result in html or Excel formats that can then be saved onto your computer. 
3.	Send this same data to the .NET team at Microsoft so that we can determine which APIs are needed by the most people.

The data we are collecting is the same as what is in the spreadsheet. None of your source code or binaries will be sent from your machine to Microsoft. 

###Using the Portability Analyzer

Use the following instructions to run Portability Analyzer.
1.	Go to the GitHub repo for the portability analyzer.
2.	Download the .zip file and unzip it, then run the .exe file.
3.	In the *Path to application folder* text box enter the directory path to your .csproj file (either by inserting a path string or clicking Browse button and navigating to the folder).
4.	Choose desired *Build configuration* and *Build platform* for MSBuild of desired project.
5.	Click the **Analyze** button.
6.	After the analysis is complete, a report of how portable your app is right now to .NET Core 3.0 will be presented in the UI.
7.	By pressing the **Export** button you can export the analysis to your computer in the desired format. 


###Troubleshooting

If your project is not built, an error warning will pop up to remind you to build your project so that the .exe file can be found by the analyzer.


###Summary

Please download the .zip file and use the Portability Analyzer .exe on your desktop applications. It will help you determine how compatible your apps are with .NET Core 3.0.
