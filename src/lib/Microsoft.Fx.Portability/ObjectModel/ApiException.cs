// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.Fx.Portability.ObjectModel
{
    /// <summary>
    /// A single ApiException used for holding the data of the single exception.
    /// Includes override methods for Equals and ToString.
    /// </summary>
    public class ApiException
    {
        public string Exception { get; set; }

        public string RID { get; set; }

        public string Platform { get; set; }

        public string Version { get; set; }

        public override string ToString()
        {
            return $"{Exception}:{RID}_{Platform},v{Version}";
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as ApiException);
        }

        public virtual bool Equals(ApiException exc)
        {
            return exc != null && ToString().Equals(exc.ToString(), System.StringComparison.Ordinal);
        }

        public override int GetHashCode()
        {
            return ToString().GetHashCode();
        }
    }
}
