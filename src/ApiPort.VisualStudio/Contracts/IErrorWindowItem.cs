using Microsoft.VisualStudio.Shell;

namespace ApiPortVS.Contracts
{
    public interface IErrorWindowItem
    {
        ErrorTask ErrorWindowTask { get; }
    }
}