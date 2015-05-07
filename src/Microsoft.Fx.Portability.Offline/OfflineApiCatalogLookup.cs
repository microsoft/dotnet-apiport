// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Fx.Portability.ObjectModel;
using System;

namespace Microsoft.Fx.Portability
{
    public class OfflineApiCatalogLookup : CloudApiCatalogLookup
    {
        public OfflineApiCatalogLookup(IProgressReporter progressReporter)
            : base(GetData(progressReporter))
        { }

        private static DotNetCatalog GetData(IProgressReporter progressReporter)
        {
            using (var progressTask = progressReporter.StartTask("Loading catalog"))
            {
                try
                {
                    return Data.LoadCatalog();
                }
                catch (Exception)
                {
                    progressTask.Abort();
                    throw;
                }
            }
        }
    }
}
