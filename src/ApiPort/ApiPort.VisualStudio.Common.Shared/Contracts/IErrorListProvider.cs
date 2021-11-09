// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using ApiPortVS.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ApiPortVS.Contracts
{
    /// <summary>
    /// Provides functionality for the Error List in Visual Studio.
    /// </summary>
    public interface IErrorListProvider
    {
        /// <summary>
        /// Displays given source mapped items in the error window.
        /// </summary>
        Task DisplaySourceItemsAsync(IEnumerable<ISourceMappedItem> items, ICollection<CalculatedProject> projects);
    }
}
