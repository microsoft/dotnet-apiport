// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using ApiPort.Resources;
using Microsoft.Fx.Portability.ObjectModel;
using System.Linq;

namespace ApiPort.CommandLine
{
    internal class AnalyzeOptionSet : ServiceEndpointOptionSet
    {
        public AnalyzeOptionSet(string name)
            : base(name, AppCommands.AnalyzeAssemblies)
        {
            Add("f|file=", LocalizedStrings.ListOfAssembliesToAnalyze, UpdateInputAssemblies, true);
            Add("o|out=", LocalizedStrings.OutputFileName, e => OutputFileName = e);
            Add("d|description=", LocalizedStrings.DescriptionHelp, e => Description = e);
            Add("t|target=", LocalizedStrings.TargetsToCheckAgainst, UpdateTargets);
            Add("r|resultFormat=", LocalizedStrings.ResultFormatHelp, UpdateOutputFormats);
            Add("p|showNonPortableApis", LocalizedStrings.CmdHelpShowNonPortableApis, e => { if (e != null) { RequestFlags |= AnalyzeRequestFlags.ShowNonPortableApis; } });
            Add("b|showBreakingChanges", LocalizedStrings.CmdHelpShowBreakingChanges, e => { if (e != null) { RequestFlags |= AnalyzeRequestFlags.ShowBreakingChanges; } });
            Add("noDefaultIgnoreFile", LocalizedStrings.CmdHelpNoDefaultIgnoreFile, e => { if (e != null) { RequestFlags |= AnalyzeRequestFlags.NoDefaultIgnoreFile; } });
            Add("i|ignoreAssemblyFile=", LocalizedStrings.CmdHelpIgnoreAssembliesFile, UpdateIgnoredAssemblyFiles);
            Add("s|suppressBreakingChange=", LocalizedStrings.CmdHelpSuppressBreakingChange, UpdateBreakingChangeSuppressions);
        }

        protected override bool ValidateValues()
        {
            // If nothing is set, default to ShowNonPortableApis
            if ((RequestFlags & (AnalyzeRequestFlags.ShowBreakingChanges | AnalyzeRequestFlags.ShowNonPortableApis)) == AnalyzeRequestFlags.None)
            {
                RequestFlags |= AnalyzeRequestFlags.ShowNonPortableApis;
            }

            // If no output formats have been supplied, default to Excel
            // TODO: Should probably get this from the service, not hard-coded
            if (!OutputFormats.Any())
            {
                UpdateOutputFormats("Excel");
            }

            return InputAssemblies.Any();
        }
    }
}
