// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using ApiPortVS.Resources;
using Microsoft.VisualStudio.Shell;
using System;
using System.Runtime.InteropServices;

namespace ApiPortVS
{
    /// <summary>
    /// This class implements the tool window exposed by this package and hosts a user control.
    /// </summary>
    /// <remarks>
    /// In Visual Studio tool windows are composed of a frame (implemented by the shell) and a pane,
    /// usually implemented by the package implementer.
    /// <para>
    /// This class derives from the ToolWindowPane class provided from the MPF in order to use its
    /// implementation of the IVsUIElementPane interface.
    /// </para>
    /// </remarks>
    [Guid("d8089baf-ab5c-4068-83ea-aba62a8d752a")]
    public class AnalysisOutputToolWindow : ToolWindowPane
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AnalysisOutputToolWindow"/> class.
        /// </summary>
        public AnalysisOutputToolWindow() : base(null)
        {
            this.Caption = LocalizedStrings.PortabilityAnalysisResults;

            // This is the user control hosted by the tool window; Note that, even if this class implements IDisposable,
            // we are not calling Dispose on this object. This is because ToolWindowPane calls Dispose on
            // the object returned by the Content property.
            this.Content = new AnalysisOutputToolWindowControl();
        }
    }
}
