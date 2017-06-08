// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using ApiPortVS.Contracts;
using EnvDTE;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;

using VisualStudio = Microsoft.VisualStudio.Shell;

namespace ApiPortVS
{
    public class OutputWindowWriter : TextWriter, IOutputWindowWriter
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

        public async Task ShowWindowAsync()
        {
            await VisualStudio.ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            _outputWindow.Activate();

            try
            {
                Window window = _dte.Windows.Item(Constants.vsWindowKindOutput);
                window.Activate();
            }
            catch (Exception) { }
        }

        public async Task ClearWindowAsync()
        {
            await VisualStudio.ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
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

        /// <summary>
        /// Solution to not being able to embed interop types into assembly
        /// https://blogs.msdn.microsoft.com/mshneer/2009/12/07/vs-2010-compiler-error-interop-type-xxx-cannot-be-embedded-use-the-applicable-interface-instead/
        /// </summary>
        private static class Constants
        {
            /// <summary>
            /// <see cref="EnvDTE.Constants.vsWindowKindOutput"/>
            /// https://msdn.microsoft.com/en-us/library/envdte.constants.vswindowkindoutput.aspx?f=255&MSPPError=-2147217396
            /// </summary>
            public const string vsWindowKindOutput = "{34E76E81-EE4A-11D0-AE2E-00A0C90FFFC3}";
        }
    }
}
