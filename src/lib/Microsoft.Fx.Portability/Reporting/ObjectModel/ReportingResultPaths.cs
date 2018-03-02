// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;

namespace Microsoft.Fx.Portability.Reporting.ObjectModel
{
    public struct ReportingResultPaths
    {
        public IEnumerable<string> Paths { get; set; }

        public ReportingResult Result { get; set; }
    }
}
