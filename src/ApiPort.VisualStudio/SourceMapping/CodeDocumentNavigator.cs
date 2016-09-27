// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.TextManager.Interop;

namespace ApiPortVS.SourceMapping
{
    internal static class CodeDocumentNavigator
    {
        private static Guid s_logicalViewGuid = VSConstants.LOGVIEWID.Code_guid;

        public static void Navigate(object sender, EventArgs e)
        {
            var task = sender as Task;
            if (task != null)
            {
                OpenDocumentTo(task.Document, task.Line, task.Column);
            }
        }

        public static void OpenDocumentTo(string documentPath, int line, int column)
        {
            var openDocument = Package.GetGlobalService(typeof(IVsUIShellOpenDocument)) as IVsUIShellOpenDocument;
            if (openDocument == null)
            {
                return;
            }

            var window = GetFrameForDocument(documentPath);
            if (window == null)
            {
                return;
            }

            object pvar;
            window.GetProperty((int)__VSFPROPID.VSFPROPID_DocData, out pvar);

            var buffer = pvar as VsTextBuffer;
            if (buffer == null)
            {
                buffer = GetBufferFromProvider(pvar);
            }

            var textManager = Package.GetGlobalService(typeof(VsTextManagerClass)) as IVsTextManager;
            if (textManager != null && buffer != null)
            {
                textManager.NavigateToLineAndColumn(buffer, ref s_logicalViewGuid, line, column, line, column);
            }
        }

        private static IVsWindowFrame GetFrameForDocument(string documentPath)
        {
            var openDocument = Package.GetGlobalService(typeof(IVsUIShellOpenDocument)) as IVsUIShellOpenDocument;

            IVsWindowFrame window = null;
            Microsoft.VisualStudio.OLE.Interop.IServiceProvider serviceProvider;
            IVsUIHierarchy hierarchy;
            uint itemId;
            try
            {
                ErrorHandler.ThrowOnFailure(openDocument.OpenDocumentViaProject
                    (documentPath, ref s_logicalViewGuid, out serviceProvider, out hierarchy, out itemId, out window));
            }
            catch
            {
                return null;
            }

            return window;
        }

        private static VsTextBuffer GetBufferFromProvider(object pvar)
        {
            var bufferProvider = pvar as IVsTextBufferProvider;
            if (bufferProvider == null)
            {
                return null;
            }

            IVsTextLines lines;
            bufferProvider.GetTextBuffer(out lines);
            return lines as VsTextBuffer;
        }
    }
}