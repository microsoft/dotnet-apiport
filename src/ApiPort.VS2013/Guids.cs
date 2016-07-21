using System;

namespace ApiPortVS
{
    static class Guids
    {
        public const string ApiPortVSPkgString = "05b3d9f1-6699-4f10-a9e4-da2ed1248523";
        public const string AnalyzeMenuItemCmdSetString = "d4c08529-d7a0-4ee1-8093-fb5d4b54e36c";
        public const string ProjectContextMenuItemCmdSetString = "5AD32A4E-F8D4-4675-9914-CD514C32FF6D";

        public static readonly Guid analyzeMenuItemCmdSet = new Guid(AnalyzeMenuItemCmdSetString);
        public static readonly Guid projectContextMenuItemCmdSet = new Guid(ProjectContextMenuItemCmdSetString);
    }
}