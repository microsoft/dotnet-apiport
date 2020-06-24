// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace Microsoft.Fx.Portability.ObjectModel
{
    /// <summary>
    /// Catalog used for hold additional data not from ApiCatalog.
    /// To be used to easily add data into the pipeline without having to change existing models.
    /// Each field needs to be individually retrieved by the portability service and will likely be stored their own file/blob.
    /// </summary>
    public class AdditionalDataCatalog
    {
        public IEnumerable<ApiExceptionStorage> Exceptions { get; set; }
    }
}
