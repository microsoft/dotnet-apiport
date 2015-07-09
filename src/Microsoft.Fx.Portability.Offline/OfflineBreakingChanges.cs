// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;

namespace Microsoft.Fx.Portability
{
    public class OfflineBreakingChanges : IEnumerable<BreakingChange>
    {
        private readonly IEnumerable<BreakingChange> _breakingChanges;

        public OfflineBreakingChanges(IProgressReporter progressReporter)
        {
            using (var progressTask = progressReporter.StartTask("Loading breaking changes"))
            {
                try
                {
                    _breakingChanges = Data.LoadBreakingChanges();
                }
                catch (Exception)
                {
                    progressTask.Abort();
                    throw;
                }
            }
        }

        public IEnumerator<BreakingChange> GetEnumerator()
        {
            return _breakingChanges.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return ((System.Collections.IEnumerable)_breakingChanges).GetEnumerator();
        }
    }
}
