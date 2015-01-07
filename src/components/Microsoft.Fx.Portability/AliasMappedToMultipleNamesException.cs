using System;
using System.Collections.Generic;
using System.Globalization;
using Microsoft.Fx.Portability.Resources;

namespace Microsoft.Fx.Portability
{
    public class AliasMappedToMultipleNamesException : PortabilityAnalyzerException
    {
        static string listSeparator = CultureInfo.CurrentCulture.TextInfo.ListSeparator + " ";

        private static string GenerateMessage(IEnumerable<string> invalidNames)
        {
            return String.Format(LocalizedStrings.AliasMappedToMultipleNamesInvalidAliases, String.Join(listSeparator, invalidNames));
        }

        public AliasMappedToMultipleNamesException(IEnumerable<string> invalidNames)
            : base(GenerateMessage(invalidNames))
        { }

    }
}
