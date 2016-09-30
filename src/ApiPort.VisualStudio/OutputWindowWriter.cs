// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using EnvDTE;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.Diagnostics;
using System.IO;
using System.Text;

using static EnvDTE.Constants;

namespace ApiPortVS
{
    public class OutputWindowWriter : TextWriter
    {
        private readonly DTE _dte;
        private readonly IVsOutputWindowPane _outputWindow;

        public OutputWindowWriter(DTE dte, IVsOutputWindowPane outputWindow)
        {
            _outputWindow = outputWindow;
            _dte = dte;

            _outputWindow.Clear();
        }

        public override Encoding Encoding { get { return Encoding.UTF8; } }

        public void ShowWindow()
        {
            _outputWindow.Activate();

            try
            {
                Window window = _dte.Windows.Item(vsWindowKindOutput);
                window.Activate();
            }
            catch (Exception) { }
        }

        public override void Write(char text)
        {
            var errCode = _outputWindow.OutputStringThreadSafe(text.ToString());

            if (ErrorHandler.Failed(errCode))
            {
                Debug.WriteLine("Failed to write on the Output window");
            }
            else
            {
                _outputWindow.Activate();
            }
        }
    }
}
