// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using ApiPortVS.Contracts;
using ApiPortVS.ViewModels;
using System.Collections.Generic;
using System.Threading.Tasks;

using VisualStudio = Microsoft.VisualStudio.Shell;

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

        public async Task ViewAsync(IEnumerable<string> urls)
        {
            await _toolbar.ShowToolbarAsync().ConfigureAwait(false);

            await VisualStudio.ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            foreach (var url in urls)
            {
                _model.Paths.Add(url);
            }
        }
    }
}
