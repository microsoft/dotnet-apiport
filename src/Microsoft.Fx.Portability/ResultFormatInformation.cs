// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;

namespace Microsoft.Fx.Portability
{
    public class ResultFormatInformation
    {
        public string DisplayName { get; set; }
        public string MimeType { get; set; }

        public override bool Equals(object obj)
        {
            var other = obj as ResultFormatInformation;

            if (other == null)
            {
                return false;
            }

            return string.Equals(other.DisplayName, DisplayName, StringComparison.OrdinalIgnoreCase);
        }

        public override int GetHashCode()
        {
            return DisplayName.GetHashCode();
        }
    }
}
