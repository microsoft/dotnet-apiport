// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;

namespace Microsoft.Fx.Portability.ObjectModel
{
    public interface IApiRecommendations
    {
        IEnumerable<ApiNote> GetNotes(string docId);

        IEnumerable<BreakingChange> GetBreakingChanges(string docId);

        string GetRecommendedChanges(string docId);

        string GetSourceCompatibleChanges(string docId);

        string GetComponent(string docId);
    }
}
