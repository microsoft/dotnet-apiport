// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Versioning;

namespace Microsoft.Fx.Portability.ObjectModel
{
    [DataContract]
    public sealed class AnalyzeResponseV1 : IComparable
    {
        public AnalyzeResponseV1()
        { }

        public AnalyzeResponseV1(AnalyzeResponse response)
        {
            MissingDependencies = response.MissingDependencies.Select(m => new MemberInfoV1(m)).ToList();
            UnresolvedUserAssemblies = response.UnresolvedUserAssemblies;
            Targets = response.Targets;
            SubmissionId = response.SubmissionId;
        }

        [DataMember]
        public IList<MemberInfoV1> MissingDependencies { get; set; }

        [DataMember]
        public IList<string> UnresolvedUserAssemblies { get; set; }

        [DataMember]
        public IList<FrameworkName> Targets { get; set; }

        [DataMember]
        public string SubmissionId { get; set; }

        public int CompareTo(object obj)
        {
            var analyzeObject = obj as AnalyzeResponseV1;
            return SubmissionId.CompareTo(analyzeObject.SubmissionId);
        }
    }
}
