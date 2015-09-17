// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace ApiPort
{
    public enum AppCommands
    {
        ListTargets,
        AnalyzeAssemblies,
        ListOutputFormats,
#if DOCID_SEARCH
        DocIdSearch
#endif
        Help,
        Exit
    }
}
