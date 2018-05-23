// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Fx.Portability.ObjectModel;

namespace Microsoft.Fx.Portability.Azure.ObjectModel.TableStorage
{
    public class MemberAggregate : BaseEntity
    {
        public string DocId { get; set; }
        public int UsageCount { get; set; }

        public MemberAggregate(MemberInfo member, int usageCount)
        {
            DocId = member.MemberDocId + (member.TypeDocId == null ? string.Empty : (":" + member.TypeDocId));
            UsageCount = usageCount;
            PartitionKey = RowKey = ComputeHashCode(DocId);
        }

        public MemberAggregate (string docId, int usageCount)
        {
            DocId = docId;
            UsageCount = usageCount;
            PartitionKey = RowKey = ComputeHashCode(docId);
        }

        public MemberAggregate()
        {

        }
    }
}
