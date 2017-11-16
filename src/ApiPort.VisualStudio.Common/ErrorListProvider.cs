// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using ApiPortVS.Contracts;
using ApiPortVS.Models;
using Microsoft.Fx.Portability.Reporting;
using Microsoft.VisualStudio.Shell.Interop;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ApiPortVS
{
    public class ErrorListProvider : IErrorListProvider
    {
        private readonly Microsoft.VisualStudio.Shell.ErrorListProvider _errorList;
        private readonly IFileSystem _fileSystem;
        private readonly IVSThreadingService _threadingService;

        public ErrorListProvider(
            Microsoft.VisualStudio.Shell.ErrorListProvider errorList,
            IFileSystem fileSystem,
            IVSThreadingService threadingService)
        {
            _fileSystem = fileSystem;
            _errorList = errorList;
            _threadingService = threadingService;
        }

        public async Task DisplaySourceItemsAsync(IEnumerable<ISourceMappedItem> items, ICollection<CalculatedProject> projects)
        {
            await _threadingService.SwitchToMainThreadAsync();

            _errorList.Tasks.Clear();
            _errorList.Refresh();
            _errorList.SuspendRefresh();

            var projectWithOutputMappings = new ConcurrentDictionary<string, IVsHierarchy>();

            foreach (var calculatedProject in projects)
            {
                var project = calculatedProject.Project;
                var outputs = calculatedProject.OutputFiles;

                if (outputs == null)
                {
                    continue;
                }

                var hierarchy = calculatedProject.VsHierarchy;

                foreach (var output in outputs)
                {
                    projectWithOutputMappings.AddOrUpdate(output, hierarchy, (existingKey, existingValue) => hierarchy);
                }
            }

            await _threadingService.SwitchToMainThreadAsync();

            try
            {
                var defaultHierarchy = projects.First().VsHierarchy;

                foreach (var item in items)
                {
                    if (!_fileSystem.FileExists(item.Path))
                    {
                        continue;
                    }

                    if (!projectWithOutputMappings.TryGetValue(item.Assembly, out IVsHierarchy hierarchy))
                    {
                        hierarchy = defaultHierarchy;
                    }

                    var errorWindowTask = item.GetErrorWindowTask(hierarchy);
                    var result = _errorList.Tasks.Add(errorWindowTask);
                }
            }
            finally
            {
                _errorList.ResumeRefresh();
            }

            // Outside the finally because it will obscure errors reported on the output window
            _errorList.BringToFront();
        }
    }
}
