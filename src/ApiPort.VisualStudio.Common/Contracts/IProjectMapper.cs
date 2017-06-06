using EnvDTE;
using Microsoft.VisualStudio.Shell.Interop;
using System.Threading.Tasks;

namespace ApiPortVS.Contracts
{
    /// <summary>
    /// Maps a <see cref="Project"/> to its associated COM types in Visual Studio
    /// </summary>
    public interface IProjectMapper
    {
        Task<IVsHierarchy> GetVsHierarchyAsync(Project project);
    }
}
