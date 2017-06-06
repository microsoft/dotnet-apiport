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
    }
}
