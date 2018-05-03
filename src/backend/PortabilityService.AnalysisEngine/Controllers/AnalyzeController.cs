// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Fx.Portability.Analyzer;
using Microsoft.Fx.Portability.ObjectModel;

namespace PortabilityService.AnalysisEngine.Controllers
{
    [Route("api/[controller]")]
    public class AnalyzeController : Controller
    {
        private readonly ILogger<AnalyzeController> _logger;
        private readonly IConfiguration _configuration;
        //TODO: inject
        //private readonly IRequestAnalyzer _requestAnalyzer;
        private readonly IStorage _storage;

        public AnalyzeController(
            IConfiguration configuration,
            //TODO: inject
            //IRequestAnalyzer requestAnalyzer,
            IStorage storage,
            ILogger<AnalyzeController> logger)
        {
            _configuration = configuration;
            //TODO: inject
            //_requestAnalyzer = requestAnalyzer;
            _storage = storage;
            _logger = logger;
        }

        [Route("")]
        [HttpPost]
        public async Task<IActionResult> Analyze(string submissionId)
        {
            _logger.LogInformation("{controller}-{action}", nameof(AnalyzeController), nameof(Analyze));
            try
            {
                if (string.IsNullOrEmpty(submissionId))
                {
                    _logger.LogError("Submission Id should not be null or empty");
                    return NotFound();
                }

                var request = await _storage.RetrieveRequestAsync(submissionId);

                if (request == null)
                {
                    _logger.LogError("Request for {submissionId} not found", submissionId);
                    return NotFound();
                }

                // if the user opted out of us collecting telemetry
                if (!request.RequestFlags.HasFlag(AnalyzeRequestFlags.NoTelemetry))
                {
                    //TODO: remove the blob from Azure Blob Storage
                }

                var result = await AnalyzeRequestAsync(request, submissionId);

                await _storage.SaveResultToBlobAsync(submissionId, result);

                return Ok();
            }
            catch (Exception exception)
            {
                _logger.LogError("Error occurs when analyzing request from submission '{submissionId}': {exception}", submissionId, exception);
                throw;
            }
        }

        private async Task<AnalyzeResponse> AnalyzeRequestAsync(AnalyzeRequest analyzeRequest, string submissionId)
        {
            using (_logger.BeginScope($"Analyzing request for {submissionId}"))
            {
                // If no request flags are specified, assume portability report.  This option is shown in newer clients
                // but older clients (alpha, VS plugin) as of 3/13/2015 do not show this and will submit a request with
                // no specified reports
                if (analyzeRequest.RequestFlags == AnalyzeRequestFlags.None || analyzeRequest.RequestFlags == AnalyzeRequestFlags.NoTelemetry)
                {
                    analyzeRequest.RequestFlags |= AnalyzeRequestFlags.ShowNonPortableApis;
                }

                //TODO: invoke the real analysis engine to do the work
                //return _requestAnalyzer.AnalyzeRequest(analyzeRequest, submissionId);
                await Task.Yield(); // to remove the warning about missing await in an async method

                return new AnalyzeResponse
                {
                    MissingDependencies = new System.Collections.Generic.List<MemberInfo>
                    {
                        new MemberInfo { MemberDocId = "doc1" },
                        new MemberInfo { MemberDocId = "doc2" }
                    },
                    SubmissionId = Guid.NewGuid().ToString(),
                    Targets = new System.Collections.Generic.List<System.Runtime.Versioning.FrameworkName>
                    {
                        new System.Runtime.Versioning.FrameworkName("target1", Version.Parse("1.0.0.0"))
                    },
                    UnresolvedUserAssemblies = new System.Collections.Generic.List<string>
                    {
                        "assembly1",
                        "assembly2",
                        "assembly3"
                    }
                };
            }
        }
    }
}
