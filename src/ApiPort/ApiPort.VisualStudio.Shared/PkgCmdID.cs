// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace ApiPortVS
{
    /// <summary>
    /// Command ids for Visual Studio menu items
    /// MUST match PkgCmdID.h.
    /// </summary>
    internal static class PkgCmdID
    {
        public const uint CmdIdAnalyzeMenuItem = 0x0100;
        public const uint CmdIdAnalyzeOptionsMenuItem = 0x0101;
        public const uint CmdIdAnalyzeToolbarMenuItem = 0x0102;
        public const uint CmdIdProjectContextMenuItem = 0x0200;
        public const uint CmdIdProjectContextDependentsMenuItem = 0x0201;
        public const uint CmdIdProjectContextOptionsMenuItem = 0x0202;
        public const uint CmdIdSolutionContextMenuItem = 0x0300;
        public const uint CmdIdSolutionContextOptionsMenuItem = 0x0301;
    }
}
