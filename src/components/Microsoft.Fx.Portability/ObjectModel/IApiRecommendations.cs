using Microsoft.Fx.Portability;
using System.Collections.Generic;

namespace Microsoft.Fx.Portability.ObjectModel
{
    public interface IApiRecommendations
    {
        IEnumerable<ApiNote> GetNotes(string docId);

        string GetRecommendedChanges(string docId);

        string GetSourceCompatibleChanges(string docId);

        string GetComponent(string docId);
    }
}
