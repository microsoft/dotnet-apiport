// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;

namespace Microsoft.Fx.Portability.ObjectModel
{
#if FEATURE_SERIALIZABLE
    [Serializable]
#endif
    public class DiagnosticAnalyzerInfo
    {
        private static readonly Regex s_regEx = new Regex(@"(\d+)", RegexOptions.Compiled);

        public string AnalyzerName { get; set; }
        public string Id { get; set; }
        public ICollection<CompatibilityRange> CompatibilityRanges { get; set; }
        public bool IsCompatibilityDiagnostic { get; set; }
        public ICollection<string> SupportedLanguages { get; set; }

        /// <summary>
        /// Returns the integer number value from the Id string property or a -1 if an integer value cannot be returned. 
        /// </summary>
        public int GetIdNumber()
        {
            if (string.IsNullOrEmpty(Id))
            {
                return -1;
            }

            var regExMatches = s_regEx.Match(Id);
            return regExMatches.Success ? int.Parse(regExMatches.Captures[0].Value, CultureInfo.InvariantCulture) : -1;
        }
    }
}
