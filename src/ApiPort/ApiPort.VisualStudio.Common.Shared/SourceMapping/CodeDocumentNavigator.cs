// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.TextManager.Interop;
using System;

#if DEV15
using TaskListItem = Microsoft.VisualStudio.Shell.Task;
#endif

namespace ApiPortVS.SourceMapping
{
    internal static class CodeDocumentNavigator
    {
        private static Guid _logicalViewGuid = VSConstants.LOGVIEWID.Code_guid;

        public static void Navigate(object sender, EventArgs e)
        {
            if (sender is TaskListItem task)
            {
                OpenDocumentTo(task.Document, task.Line, task.Column);
            }
        }

        public static void OpenDocumentTo(string documentPath, int line, int column)
        {
            if (!(Package.GetGlobalService(typeof(IVsUIShellOpenDocument)) is IVsUIShellOpenDocument openDocument))
            {
                return;
            }

            var window = GetFrameForDocument(documentPath);
            if (window == null)
            {
                return;
            }

            window.GetProperty((int)__VSFPROPID.VSFPROPID_DocData, out var pvar);

            if (!(pvar is VsTextBuffer buffer))
            {
                buffer = GetBufferFromProvider(pvar);
            }

            if (Package.GetGlobalService(typeof(VsTextManagerClass)) is IVsTextManager textManager
                && buffer != null)
            {
                textManager.NavigateToLineAndColumn(buffer, ref _logicalViewGuid, line, column, line, column);
            }
        }

        private static IVsWindowFrame GetFrameForDocument(string documentPath)
        {
            var openDocument = Package.GetGlobalService(typeof(IVsUIShellOpenDocument)) as IVsUIShellOpenDocument;

            IVsWindowFrame window = null;
            try
            {
                ErrorHandler.ThrowOnFailure(openDocument.OpenDocumentViaProject(
                    documentPath,
                    ref _logicalViewGuid,
                    out var serviceProvider,
                    out var hierarchy,
                    out var itemId,
                    out window));
            }
            catch
            {
                return null;
            }

            return window;
        }

        private static VsTextBuffer GetBufferFromProvider(object pvar)
        {
            if (!(pvar is IVsTextBufferProvider bufferProvider))
            {
                return null;
            }

            bufferProvider.GetTextBuffer(out var lines);
            return lines as VsTextBuffer;
        }
    }
}
