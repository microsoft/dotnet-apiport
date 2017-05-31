using EnvDTE;
using System;
using System.Collections.Generic;

namespace ApiPortVS.Models
{
    /// <summary>
    /// Contains a VS Project with its calculated outputs
    /// </summary>
    public class CalculatedProject
    {
        public CalculatedProject(Project project, IEnumerable<string> outputFiles)
        {
            Project = project ?? throw new ArgumentNullException(nameof(project));
            OutputFiles = outputFiles ?? throw new ArgumentNullException(nameof(outputFiles));
        }

        public Project Project { get; }

        public IEnumerable<string> OutputFiles { get; }
    }
}
