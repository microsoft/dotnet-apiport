// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;

namespace Microsoft.Fx.OpenXmlExtensions
{
    internal class HyperlinkCell
    {
        public string DisplayString { get; set; }

        public Uri Url { get; set; }

        public uint? StyleIndex { get; set; }
    }
}
