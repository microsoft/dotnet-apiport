using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiPortVS.Contracts
{
    public interface IOutputWindowWriter
    {
        Task ShowWindowAsync();

        Task ClearWindowAsync();

        void WriteLine();

        void WriteLine(string contents);

        void WriteLine(string format, params object[] arg);
    }
}
