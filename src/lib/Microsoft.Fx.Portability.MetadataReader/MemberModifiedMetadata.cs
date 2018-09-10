// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.Fx.Portability.Analyzer
{
    internal class MemberModifiedMetadata
    {
        public bool IsRequired { get; }

        public MemberMetadataInfo Metadata { get; }

        public MemberModifiedMetadata(bool isRequired, MemberMetadataInfo metadata)
        {
            IsRequired = isRequired;
            Metadata = metadata;
        }
    }
}
