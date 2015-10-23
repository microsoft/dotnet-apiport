# Offline Mode

There are times when using the web service is not ideal (extremely large data sets, limited network connectivity, privacy concerns) and
there is a mode that can enable offline analysis of projects. This is not listed on the release downloads, but can be built manually. 
The steps to do this:

1. Clone the project: `git clone https://github.com/Microsoft/dotnet-apiport`
2. Build the project: `build.cmd`. 

	*Note: This command must be used as it gathers the correct assemblies for offline mode. Building in VS does not do this.*
	
3. Go to `bin\release\ApiPort.Offline`
4. Run `ApiPort.exe` from this directory as normal.

## Report writers

Additional reports can be generated in offline mode. Just implement `Microsoft.Fx.Portability.Reporting.IReportWriter` and add the correct
entry to unity.config. The offline mode will pick it up and allow reports to be returned in custom formats.