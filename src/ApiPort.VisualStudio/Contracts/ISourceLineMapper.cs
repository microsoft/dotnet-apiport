using Microsoft.Fx.Portability.Reporting.ObjectModel;
using System.Collections.Generic;

namespace ApiPortVS.Contracts
{
    public interface ISourceLineMapper
    {
        /// <summary>
        /// Finds source information from the report
        /// </summary>
        /// <param name="assemblyPath"></param>
        /// <param name="pdbPath"></param>
        /// <param name="report"></param>
        /// <returns></returns>
        IEnumerable<ISourceMappedItem> GetSourceInfo(string assemblyPath, string pdbPath, ReportingResult report);

        /// <summary>
        /// Finds source information from the report
        /// </summary>
        /// <remarks>
        /// This method assumes that the pdb files for targetAssemblies will be the same path replaced with pdb.
        /// </remarks>
        /// <param name="targetAssemblies"></param>
        /// <param name="analysis"></param>
        /// <returns></returns>
        IEnumerable<ISourceMappedItem> GetSourceInfo(IEnumerable<string> targetAssemblies, ReportingResult analysis);
    }
}