// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

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
