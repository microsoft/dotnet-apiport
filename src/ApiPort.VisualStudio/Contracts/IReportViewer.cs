// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;

namespace ApiPortVS.Contracts
{
    public interface IReportViewer
    {
        void View(IEnumerable<string> urls);
    }
}
