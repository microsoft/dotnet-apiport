// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Fx.Portability;
using Microsoft.Fx.Portability.Analyzer;
using Microsoft.Fx.Portability.ObjectModel;

namespace PortabilityService.AnalysisEngine
{
    public class DemoRequestAnalyzer : IRequestAnalyzer
    {
        public AnalyzeResult AnalyzeRequest(AnalyzeRequest request, string submissionId)
        {
            using (var stream = typeof(DemoRequestAnalyzer).Assembly.GetManifestResourceStream("apiport-demo.dll.json"))
            {
                return DataExtensions.Deserialize<AnalyzeResult>(stream);
            }
        }
    }
}
