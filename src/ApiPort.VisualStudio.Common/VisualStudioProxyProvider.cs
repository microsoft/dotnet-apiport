// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Fx.Portability.Proxy;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace ApiPortVS
{
    public class VisualStudioProxyProvider : IProxyProvider
    {
        private readonly IVsWebProxy _vsWebProxy;

        public VisualStudioProxyProvider(IVsWebProxy vsWebProxy)
        {
            _vsWebProxy = vsWebProxy;
        }

        public bool CanUpdateCredentials => true;

        public IWebProxy GetProxy(Uri sourceUri) => WebRequest.DefaultWebProxy;

        public async Task<bool> TryUpdateCredentialsAsync(Uri uri, IWebProxy proxy, CredentialRequestType type, CancellationToken cancellationToken)
        {
            // This value will cause the web proxy service to first attempt to retrieve 
            // credentials from its cache and fall back to prompting if necessary. 
            const __VsWebProxyState oldState = __VsWebProxyState.VsWebProxyState_DefaultCredentials;

            var newState = (uint)__VsWebProxyState.VsWebProxyState_NoCredentials;
            var result = 0;

            // This must be run on the UI thread in case a prompt has to be shown
            // to retrieve the credentials
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            result = _vsWebProxy.PrepareWebProxy(uri.OriginalString, (uint)oldState, out newState, fOkToPrompt: 1);

            // If result is anything but 0 that most likely means that there was an error
            // so we will null out the DefaultWebProxy.Credentials so that we don't get
            // invalid credentials stored for subsequent requests.
            if (result != 0
                || newState == (uint)__VsWebProxyState.VsWebProxyState_Abort)
            {
                // The user might have clicked cancel, so we don't want to use the currently set
                // credentials if they are wrong.
                return false;
            }

            return true;
        }
    }
}
