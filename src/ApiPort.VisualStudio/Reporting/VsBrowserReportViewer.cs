// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using ApiPortVS.Contracts;
using ApiPortVS.Resources;
using Microsoft.Fx.Portability.Reporting;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.Diagnostics;
using System.IO;

namespace ApiPortVS.Reporting
{
    public class VsBrowserReportViewer : IReportViewer
    {
        private readonly IFileSystem _fileSystem;
        private readonly TextWriter _output;
        private readonly IVsWebBrowsingService _browserService;

        public VsBrowserReportViewer(IFileSystem fileSystem, TextWriter output, IVsWebBrowsingService webBrowsingService)
        {
            _fileSystem = fileSystem;
            _output = output;
            _browserService = webBrowsingService;
        }

        public void View(string url)
        {
            if (IsHtml(url))
            {
                ShowHtml(url);
            }
            else
            {
                Process.Start(url);
            }
        }

        private bool IsHtml(string url)
        {
            var extension = Path.GetExtension(url);

            return string.Equals(".html", extension, StringComparison.OrdinalIgnoreCase)
                || string.Equals(".htm", extension, StringComparison.OrdinalIgnoreCase);
        }

        private void ShowHtml(string url)
        {
            const uint REUSE_EXISTING_BROWSER_IF_AVAILABLE = 0;

            if (!_fileSystem.FileExists(url))
            {
                _output.WriteLine(LocalizedStrings.CannotSaveReport, url);
            }
            else if (_browserService == null)
            {
                _output.WriteLine(LocalizedStrings.CannotViewReport);
            }
            else
            {
                IVsWindowFrame browserFrame;
                var errCode = _browserService.Navigate(url, REUSE_EXISTING_BROWSER_IF_AVAILABLE, out browserFrame);
            }
        }
    }
}
