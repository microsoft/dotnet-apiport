// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Threading;

namespace Microsoft.Fx.Portability.Reporting.ObjectModel
{
    public class TargetUsageInfo
    {
        private int _callsToAvailableAPIs;
        private int _callsToUnavailableAPIs;

        public void IncrementCallsToAvailableApi()
        {
            Interlocked.Increment(ref _callsToAvailableAPIs);
        }

        public void IncrementCallsToUnavailableApi()
        {
            Interlocked.Increment(ref _callsToUnavailableAPIs);
        }

        public int GetAvailableAPICalls() => _callsToAvailableAPIs;

        public int GetUnavailableAPICalls() => _callsToUnavailableAPIs;

        public double PortabilityIndex
        {
            get
            {
                // prevent Div/0
                if (_callsToAvailableAPIs == 0 && _callsToUnavailableAPIs == 0)
                {
                    return 0;
                }

                return (double)_callsToAvailableAPIs / ((double)_callsToAvailableAPIs + _callsToUnavailableAPIs);
            }
        }
    }
}
