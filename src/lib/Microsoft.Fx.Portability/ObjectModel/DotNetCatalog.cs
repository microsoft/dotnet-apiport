// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;

namespace Microsoft.Fx.Portability.ObjectModel
{
    public class DotNetCatalog
    {
        public DateTimeOffset LastModified { get; set; }
        public string BuiltBy { get; set; }
        public IReadOnlyCollection<ApiInfoStorage> Apis { get; set; }
        public IReadOnlyCollection<string> FrameworkAssemblyIdenties { get; set; }
        public IReadOnlyCollection<TargetInfo> SupportedTargets { get; set; }
    }
}
