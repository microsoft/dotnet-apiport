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
        private readonly IResultToolbar _toolbar;

        public ToolbarListReportViewer(OutputViewModel model, IResultToolbar toolbar)
        {
            _model = model;
            _toolbar = toolbar;
        }

        public void View(IEnumerable<string> urls)
        {
            _toolbar.ShowToolbar();

            foreach (var url in urls)
            {
                _model.Paths.Add(url);
            }
        }
    }
}
