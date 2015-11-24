// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using ApiPort.Resources;
using System.Collections.Generic;

namespace ApiPort.CommandLine
{
    internal class DocIdSearchOptions : CommandLineOptions
    {
        public override string HelpMessage => LocalizedStrings.CmdDocId;

        public override string Name => "DocIdSearch";

        public override ICommandLineOptions Parse(IEnumerable<string> args)
        {
            return new DocIdSearchCommandLineOption();
        }

        private class DocIdSearchCommandLineOption : ConsoleDefaultApiPortOptions, ICommandLineOptions
        {
            public AppCommands Command => AppCommands.DocIdSearch;
        }
    }
}
