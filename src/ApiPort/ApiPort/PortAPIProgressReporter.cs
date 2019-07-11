using Microsoft.Fx.Portability;
using System;
using System.Collections.Generic;
using System.Text;

namespace PortAPIUI
{
    class PortAPIProgressReporter : IProgressReporter
    {
        public IReadOnlyCollection<string> Issues => throw new NotImplementedException();

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public void ReportIssue(string issue)
        {
            throw new NotImplementedException();
        }

        public void Resume()
        {
            throw new NotImplementedException();
        }

        public IProgressTask StartTask(string taskName, int totalUnits)
        {
            throw new NotImplementedException();
        }

        public IProgressTask StartTask(string taskName)
        {
            throw new NotImplementedException();
        }

        public void Suspend()
        {
            throw new NotImplementedException();
        }
    }
}
