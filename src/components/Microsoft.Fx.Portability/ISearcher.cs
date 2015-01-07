using System.Collections.Generic;

namespace Microsoft.Fx.Portability
{
    public interface ISearcher<T>
    {
        IEnumerable<T> Search(string query, int numberOfHits);
    }
}
