// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Globalization;
using Microsoft.Fx.Portability.Resources;

namespace Microsoft.Fx.Portability
{
    public class AliasMappedToMultipleNamesException : PortabilityAnalyzerException
    {
        private static string s_listSeparator = CultureInfo.CurrentCulture.TextInfo.ListSeparator + " ";

        private static string GenerateMessage(IEnumerable<string> invalidNames)
        {
            return string.Format(LocalizedStrings.AliasMappedToMultipleNamesInvalidAliases, string.Join(s_listSeparator, invalidNames));
        }

        public AliasMappedToMultipleNamesException(IEnumerable<string> invalidNames)
            : base(GenerateMessage(invalidNames))
        { }
    }
}
