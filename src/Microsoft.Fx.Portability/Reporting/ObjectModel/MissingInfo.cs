// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Fx.Portability.Resources;
using System;
using System.Globalization;

namespace Microsoft.Fx.Portability.Reporting.ObjectModel
{
    public class MissingInfo
    {
        public string DocId { get; set; }
        public string RecommendedChanges { get; protected set; }

        public MissingInfo(string docId)
        {
            DocId = docId;
        }

        public override int GetHashCode()
        {
            return DocId.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            MissingInfo other = obj as MissingInfo;
            return other != null && StringComparer.Ordinal.Equals(other.DocId, DocId);
        }

        protected string GenerateTargetStatusMessage(Version version)
        {
            if (version == null)
            {
                return LocalizedStrings.NotSupported;
            }
            else
            {
                return string.Format(CultureInfo.CurrentCulture, LocalizedStrings.SupportedOn, version);
            }
        }
    }
}
