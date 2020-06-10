// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;

namespace Microsoft.Fx.Portability.ObjectModel
{
    public class AdditionalDataCatalog
    {
        public IEnumerable<ApiExceptionStorage> Exceptions { get; set; }
    }
}
