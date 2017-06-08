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
        /// <param name="items"></param>
        /// <param name="projects"></param>
        /// <returns></returns>
        Task DisplaySourceItemsAsync(IEnumerable<ISourceMappedItem> items, ICollection<CalculatedProject> projects);
    }
}
