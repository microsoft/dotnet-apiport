// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;

namespace Microsoft.Fx.Portability.ObjectModel
{
    public sealed class AnalyzeResponse
    {
        public string ResultAuthToken { get; set; }
        public Uri ResultUrl { get; set; }
        public string SubmissionId { get; set; }
    }
}
