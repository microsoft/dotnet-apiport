// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using ApiPortVS.Contracts;
using ApiPortVS.ViewModels;
using System.Collections.Generic;

namespace ApiPortVS.Reporting
{
    internal class ToolbarListReportViewer : IReportViewer
    {
        private readonly OutputViewModel _model;

        public ToolbarListReportViewer(OutputViewModel model)
        {
            _model = model;
        }

        public void View(IEnumerable<string> urls)
        {
            foreach (var url in urls)
            {
                _model.Paths.Add(url);
            }
        }
    }
}
