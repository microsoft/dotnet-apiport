namespace ApiPortVS
{
    /// <summary>
    /// Solution to not being able to embed interop types into assembly
    /// https://blogs.msdn.microsoft.com/mshneer/2009/12/07/vs-2010-compiler-error-interop-type-xxx-cannot-be-embedded-use-the-applicable-interface-instead/
    /// </summary>
    internal static class Constants
    {
        internal static class EnvDTE
        {
            /// <summary>
            /// <see cref="EnvDTE.Constants.vsWindowKindOutput"/>
            /// https://msdn.microsoft.com/en-us/library/envdte.constants.vswindowkindoutput.aspx?f=255&MSPPError=-2147217396
            /// </summary>
            public const string vsWindowKindOutput = "{34E76E81-EE4A-11D0-AE2E-00A0C90FFFC3}";
        }

        /// <summary>
        /// <see cref="EnvDTE.CodeModelLanguageConstants"/>
        /// https://msdn.microsoft.com/en-us/library/envdte.codemodellanguageconstants.aspx
        /// </summary>
        internal static class CodeModelLanguageConstants
        {
            public const string vsCMLanguageVC = "{B5E9BD32-6D3E-4B5D-925E-8A43B79820B4}";
            public const string vsCMLanguageVB = "{B5E9BD33-6D3E-4B5D-925E-8A43B79820B4}";
            public const string vsCMLanguageCSharp = "{B5E9BD34-6D3E-4B5D-925E-8A43B79820B4}";
        }

        /// <summary>
        /// A set of well-known output group names.
        /// </summary>
        internal static class OutputGroups
        {
            /// <summary>
            /// MSBuild .targets file string for BuiltProjectOutputGroup.
            /// </summary>
            internal const string BuiltProject = "Built";
        }

        /// <summary>
        /// A set of well-known output group item metadata names.
        /// </summary>
        internal static class MetadataNames
        {
            /// <summary>
            /// MSBuild .targets file string for FinalOutputPath.
            /// </summary>
            internal const string FinalOutputPath = nameof(FinalOutputPath);

            /// <summary>
            /// Metadata name for file output location
            /// </summary>
            internal const string OutputLocation = "OUTPUTLOC";
        }

        /// <summary>
        /// Well-known project capabilities from:
        /// https://github.com/Microsoft/VSProjectSystem/blob/master/doc/overview/project_capabilities.md
        /// </summary>
        internal static class ProjectCapabilities
        {
            /// <summary>
            ///  Project is based on the Project System Extensibility SDK
            /// </summary>
            internal const string CPS = nameof(CPS);

            /// <summary>
            /// Indicates that the project is a typical MSBuild project (not DNX)
            /// in that it declares source items in the project itself (rather
            /// than a project.json file that assumes all files in the directory
            /// are part of a compilation).
            /// </summary>
            internal const string DeclaredSourceItems = nameof(DeclaredSourceItems);
        }
    }
}
