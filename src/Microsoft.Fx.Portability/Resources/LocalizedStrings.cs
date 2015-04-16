// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

// This is auto-generated with CreateLocalizedStrings.ps1

namespace Microsoft.Fx.Portability.Resources
{
    public static class LocalizedStrings
    {
        public static string DuplicateAliasError { get { return @"Duplicate alias: '{0}'"; } }
        public static string AliasCanotBeEqualToTargetNameError { get { return @"Alias cannot be equal to a target name: '{0}'"; } }
        public static string MalformedMap { get { return @"Malformed map: {0}"; } }
        public static string UnknownTarget { get { return @"Unknown target '{0}'"; } }
        public static string ServerEndpointMovedPermanently { get { return @"Error: The service endpoint has been deprecated. Please update the application to the latest version."; } }
        public static string InvalidDataMessage { get { return @"Invalid data"; } }
        public static string HttpExceptionMessage { get { return @"There was an unknown error attempting to reach the service.  Please try again."; } }
        public static string UnknownBadRequestMessage { get { return @"Unknown error.  Please make sure you the most up-to-date version"; } }
        public static string UnknownErrorCodeMessage { get { return @"There was an unknown error code from the service: '{0}'.  Please make sure you have the most up-to-date version."; } }
        public static string FullAssemblyIdentity { get { return @"{0}, FileVersion:{1}"; } }
        public static string ComputingReport { get { return @"Computing report."; } }
        public static string DetectingAssemblyReferences { get { return @"Detecting assembly references"; } }
        public static string IdentifyAssembliesToScan { get { return @"Identifying assemblies to scan."; } }
        public static string ProcessedFiles { get { return @"Processed {0}/{1} files."; } }
        public static string ProcessedItems { get { return @"Processed {0} items."; } }
        public static string RetrievingTargets { get { return @"Retrieving targets"; } }
        public static string SendingDataToService { get { return @"Sending data to service"; } }
        public static string ServerEndpointDeprecated { get { return @"Warning: A newer version of the application is available. Please update to the latest version to continue using this service"; } }
        public static string MemberShouldBeDefinedOnTypeException { get { return @"Member should be defined on a type"; } }
        public static string AvailableGroupedTargets { get { return @"Available Grouped Targets:"; } }
        public static string AliasMappedToMultipleNamesInvalidAliases { get { return @"Only single mappings between name and alias are supported.  The following aliases are invalid: {0}"; } }
        public static string TargetInformationGroups { get { return @"{0} ({1})"; } }
        public static string NotSupported { get { return @"Not supported"; } }
        public static string SupportedOn { get { return @"Supported: {0}+"; } }
        public static string UnauthorizedAccess { get { return @"Endpoint requires an authentication token."; } }
        public static string ProductInformationInvalidArgument { get { return @"Must only contain letters or digits"; } }
        public static string HowToSpecifyVersion { get { return @"In order to specify a version, please use the following format with the '-targets' option:
	(Target Name), Version=(Version)"; } }
        public static string InvalidFileName { get { return @"Invalid file name: '{0}'"; } }
        public static string UnknownFile { get { return @"Could not find file: '{0}'"; } }
        public static string NoFilesAvailableToUpload { get { return @"No files were found to upload."; } }
        public static string NotFoundException { get { return @"Resource was not found."; } }
        public static string ChangingFileExtension { get { return @"The given filename [{0}] has a different extension than the output format extension. Changed extension '{1}' to '{2}'."; } }
        public static string OverwriteFile { get { return @"Replaced output file '{0}'"; } }
        public static string AssemblyHeader { get { return @"Assembly"; } }
        public static string BackToSummary { get { return @"Back to Summary"; } }
        public static string Description { get { return @"Description"; } }
        public static string DetailsPageTitle { get { return @"Details"; } }
        public static string HowToReadTheExcelTable { get { return @"See 'http://go.microsoft.com/fwlink/?LinkId=397652' to learn how to read this table"; } }
        public static string HtmlReportTitle { get { return @".NET Portability Report"; } }
        public static string InvalidAssembly { get { return @"{0} is an invalid assembly."; } }
        public static string MissingAssembliesPageTitle { get { return @"Missing assemblies"; } }
        public static string MissingAssemblyStatus { get { return @"Reason"; } }
        public static string RecommendedChanges { get { return @"Recommended changes"; } }
        public static string SubmissionId { get { return @"Submission Id"; } }
        public static string PortabilitySummaryPageTitle { get { return @"Portability Summary"; } }
        public static string TargetMemberHeader { get { return @"Target member"; } }
        public static string Targets { get { return @"Targets"; } }
        public static string TargetTypeHeader { get { return @"Target type"; } }
        public static string UnresolvedUsedAssembly { get { return @"Unresolved assembly"; } }
        public static string UsedBy { get { return @"Used By"; } }
        public static string ProgressReportDone { get { return @"[Done]"; } }
        public static string ProgressReportFailed { get { return @"[Failed]"; } }
        public static string CompatibilityPageTitle { get { return @"Framework Compatibility"; } }
        public static string RetrievingOutputFormats { get { return @"Retrieving output formats."; } }
        public static string UnknownResultFormat { get { return @"Unknown output format: '{0}'"; } }
        public static string EdgeCompatIssueDescription { get { return @"Edge issues are those that will only impact a small minority of customers that use the given API in very specific ways. See issue details for more information."; } }
        public static string MajorCompatIssueDescription { get { return @"Major issues are those that are likely to impact most customers using the given feature or API."; } }
        public static string MinorCompatIssueDescription { get { return @"Minor issues are those that will only impact customers using the given feature in a particular way. See issue details for more information."; } }
        public static string RetargetingCompatIssueDescriptionPart1 { get { return @"Retargeting compatibility issues are breaking changes that only manifest when code is targeted to run on a newer .Net Framework version. This can happen if an application does not have a <a href=""https://msdn.microsoft.com/en-us/library/system.runtime.versioning.targetframeworkattribute%28v=vs.110%29.aspx"">TargetFrameworkAttribute</a> or when the application is rebuilt with a newer toolset."; } }
        public static string RetargetingCompatIssueDescriptionPart2 { get { return @"These issues are less impactful than runtime compatibility issues because they can typically be worked around easily, either by using a <a href=""https://msdn.microsoft.com/en-us/library/system.runtime.versioning.targetframeworkattribute%28v=vs.110%29.aspx"">TargetFrameworkAttribute</a> on the assembly, using a <a href=""https://msdn.microsoft.com/en-us/library/bb398202.aspx"">TargetFrameworkVersion</a> in the project file, or using older tools at build-time, depending on the particular issue. See issue details below for more information on how these breaking changes can be avoided."; } }
        public static string RetargetingCompatIssueDescriptionPlainText { get { return @"Retargeting compatibility issues are breaking changes that only manifest when code is targeted to run on a newer .Net Framework version. These issues are less impactful than runtime compatibility issues because they can typically be worked around easily, either by using a TargetFrameworkAttribute on the assembly, using a TargetFrameworkVersion in the project file, or using older tools at build-time, depending on the particular issue. See issue details below for more information on how these breaking changes can be avoided."; } }
        public static string RuntimeCompatIssueDescription { get { return @"Runtime compatibility issues are those that will occur simply by running code on a new .Net Framework version. These are the variety of breaking changes most likely to impact applications since they cannot be quirked away and do not depend on an application being recompiled."; } }
        public static string BreakingChangeDisclaimer { get { return @"Note that simply using a potentially broken API does not mean the app will necessarily suffer from the indicated breaking change. Many breaking changes are scoped to narrow corner-cases, and apps are only affected if they use the API in a very particular way. Breaking changes below have been grouped by scope (major, minor, or edge) depending on how likely the changes are to actually break user code in cases where the given API is used. Please read the 'details' column to understand the particular circumstances in which each of the listed APIs may fail."; } }
    }
}
