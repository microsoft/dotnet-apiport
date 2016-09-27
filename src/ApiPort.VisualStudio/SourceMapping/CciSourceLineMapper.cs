// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using ApiPortVS.Contracts;
using Microsoft.Cci;
using Microsoft.Cci.Extensions;
using Microsoft.Fx.Portability;
using Microsoft.Fx.Portability.Reporting;
using Microsoft.Fx.Portability.Reporting.ObjectModel;
using System.Collections.Generic;
using System.IO;

namespace ApiPortVS.SourceMapping
{
    public class CciSourceLineMapper : SourceLineMapper
    {
        public CciSourceLineMapper(IFileSystem fileSystem, TextWriter textOutputTarget, IProgressReporter progressReporter)
            : base(fileSystem, textOutputTarget, progressReporter)
        { }

        public override IEnumerable<ISourceMappedItem> GetSourceInfo(string assemblyPath, string pdbPath, ReportingResult report)
        {
            using (var host = new HostEnvironment())
            using (var pdbFs = File.OpenRead(pdbPath))
            using (var pdbReader = new PdbReader(pdbFs, host))
            {
                var metadataVisitor = new CciMetadataTraverser(report, pdbReader);
                var traverser = new MetadataTraverser
                {
                    PreorderVisitor = metadataVisitor,
                    TraverseIntoMethodBodies = true
                };

                var cciAssembly = host.LoadAssembly(assemblyPath);
                traverser.Traverse(cciAssembly);

                return metadataVisitor.FoundItems;
            }
        }
    }
}
