// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Threading;

namespace Microsoft.Fx.Progress
{
    internal sealed class ThrottledProgressMonitor : IProgressMonitor
    {
        private readonly IProgressMonitor _target;
        private readonly double _delay;
        private int _lastTicks;
        private float _pendingUnits;

        public ThrottledProgressMonitor(IProgressMonitor target)
            : this(target, TimeSpan.FromMilliseconds(50))
        {
        }

        public ThrottledProgressMonitor(IProgressMonitor target, TimeSpan delay)
        {
            _target = target;
            _delay = delay.TotalMilliseconds;
        }

        public void Dispose()
        {
            _target.Dispose();
        }

        private void ReportPendingUnits()
        {
            if (_pendingUnits <= 0.0f)
                return;

            _target.Report(_pendingUnits);
            _pendingUnits = 0.0f;
        }

        public void SetTask(string description)
        {
            _target.SetTask(description);
        }

        public void SetDetails(string description)
        {
            _target.SetDetails(description);
        }

        public void SetRemainingWork(float totalUnits)
        {
            ReportPendingUnits();
            _target.SetRemainingWork(totalUnits);
        }

        public void Report(float units)
        {
            _pendingUnits += units;

            var delta = Math.Abs(Environment.TickCount - _lastTicks);
            if (delta < _delay)
                return;

            ReportPendingUnits();

            _lastTicks = Environment.TickCount;
        }

        public CancellationToken CancellationToken
        {
            get { return _target.CancellationToken; }
        }
    }
}