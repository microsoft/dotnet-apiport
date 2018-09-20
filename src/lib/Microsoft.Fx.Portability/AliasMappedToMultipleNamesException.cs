// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Fx.Portability.Resources;
using System.Collections.Generic;
using System.Globalization;

namespace Microsoft.Fx.Portability
{
    public class AliasMappedToMultipleNamesException : PortabilityAnalyzerException
    {
        private static readonly string ListSeparator = CultureInfo.CurrentCulture.TextInfo.ListSeparator + " ";

        private static string GenerateMessage(IEnumerable<string> invalidNames)
        {
            return string.Format(CultureInfo.CurrentCulture, LocalizedStrings.AliasMappedToMultipleNamesInvalidAliases, string.Join(ListSeparator, invalidNames));
        }

        public AliasMappedToMultipleNamesException(IEnumerable<string> invalidNames)
            : base(GenerateMessage(invalidNames))
        { }
    }
}
