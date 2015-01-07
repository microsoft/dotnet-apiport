using System;
using System.Threading.Tasks;

namespace Microsoft.Fx.Portability
{
    public interface IObjectCache<TObject> : IDisposable
    {
        TObject Value { get; }
        DateTimeOffset LastUpdated { get; }
        Task UpdateAsync();
    }
}
