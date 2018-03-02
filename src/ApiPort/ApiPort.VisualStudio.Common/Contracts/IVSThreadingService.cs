// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Threading;
using System.Threading.Tasks;

namespace ApiPortVS.Contracts
{
    /// <summary>
    /// Provides threading services to safely communicate between synchronous
    /// and asynchronous tasks without freezing the UI.
    /// 
    /// Further reading:
    /// https://github.com/Microsoft/VSProjectSystem/blob/master/doc/overview/threading_model.md
    /// https://blogs.msdn.microsoft.com/andrewarnottms/2014/05/07/asynchronous-and-multithreaded-programming-within-vs-using-the-joinabletaskfactory/
    /// </summary>
    public interface IVSThreadingService
    {
        /// <summary>
        /// Ensures that the caller is on the UI thread.
        /// </summary>
        /// <returns></returns>
        Task SwitchToMainThreadAsync(CancellationToken cancellationToken = default(CancellationToken));
    }
}
