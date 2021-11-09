// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Fx.Portability.Reporting.ObjectModel;
using System.Collections.Generic;

namespace ApiPortVS.Contracts
{
    public interface ISourceLineMapper
    {
        /// <summary>
        /// Finds source information from the report.
        /// </summary>
        IEnumerable<ISourceMappedItem> GetSourceInfo(string assemblyPath, string pdbPath, ReportingResult report);

        /// <summary>
        /// Finds source information from the report.
        /// </summary>
        /// <remarks>
        /// This method assumes that the pdb files for targetAssemblies will be the same path replaced with pdb.
        /// </remarks>
        IEnumerable<ISourceMappedItem> GetSourceInfo(IEnumerable<string> targetAssemblies, ReportingResult analysis);
    }
}
