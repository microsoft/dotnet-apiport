// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace ApiPortVS
{
    public class OutputWindowWriter : TextWriter
    {
        private readonly IVsOutputWindowPane _outputWindow;

        public OutputWindowWriter(IVsOutputWindowPane outputWindow)
        {
            _outputWindow = outputWindow;

            Clear();
        }

        public override Encoding Encoding { get { return Encoding.UTF8; } }

        public void Clear()
        {
            _outputWindow.Clear();
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
