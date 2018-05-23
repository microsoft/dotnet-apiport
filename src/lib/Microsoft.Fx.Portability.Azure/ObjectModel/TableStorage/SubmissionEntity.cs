// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Fx.Portability.ObjectModel;
using System;
using System.Collections.Generic;

namespace Microsoft.Fx.Portability.Azure.ObjectModel.TableStorage
{
    public class SubmissionEntity : BaseEntity
    {
        public string AssemblyInfo { get; set; }
        public string SubmissionId { get; set; }
        public string Targets { get; set; }
        public string ApplicationName { get; set; }

        public SubmissionEntity(AssemblyInfo assembly, IEnumerable<string> targets, string appName, string submissionId)
        {
            AssemblyInfo = assembly.AssemblyIdentity + ", fileVer: " + assembly.FileVersion;
            SubmissionId = submissionId;
            Targets = String.Join(";", targets);
            ApplicationName = appName;

            PartitionKey = ComputeHashCode(AssemblyInfo);
            RowKey = submissionId;
        }
    }
}
