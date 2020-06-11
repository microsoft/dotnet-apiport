// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace Microsoft.Fx.Portability.ObjectModel
{
    public class AdditionalDataCatalog
    {
        public IEnumerable<ApiExceptionStorage> Exceptions { get; set; }
    }
}
