// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Runtime.Versioning;

namespace Microsoft.Fx.Portability.ObjectModel
{
    public class ApiInfoStorage
    {
        public string DocId { get; set; }

        public string Type { get; set; }

        public string Name { get; set; }

        public string FullName { get; set; }

        /// <summary>
        /// DocId of the parent. Null if the Api does not have a parent.
        /// </summary>
        public string Parent { get; set; }

        public IReadOnlyCollection<FrameworkName> Targets { get; set; }

        public IReadOnlyCollection<ApiMetadataStorage> Metadata { get; set; }
    }
}
