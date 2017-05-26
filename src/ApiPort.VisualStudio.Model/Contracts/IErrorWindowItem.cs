// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.VisualStudio.Shell;

namespace ApiPortVS.Contracts
{
    public interface IErrorWindowItem
    {
        ErrorTask ErrorWindowTask { get; }
    }
}