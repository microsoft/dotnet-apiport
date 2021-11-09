// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using EnvDTE;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.Collections.Generic;

namespace ApiPortVS.Models
{
    /// <summary>
    /// Contains a VS Project with its calculated outputs.
    /// </summary>
    public class CalculatedProject
    {
        public CalculatedProject(Project project, IVsHierarchy hierarchy, IEnumerable<string> outputFiles)
        {
            Project = project ?? throw new ArgumentNullException(nameof(project));
            VsHierarchy = hierarchy ?? throw new ArgumentNullException(nameof(hierarchy));
            OutputFiles = outputFiles ?? throw new ArgumentNullException(nameof(outputFiles));
        }

        public IVsHierarchy VsHierarchy { get; }

        public Project Project { get; }

        public IEnumerable<string> OutputFiles { get; }
    }
}
