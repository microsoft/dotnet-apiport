// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Fx.Portability.Analyzer;
using Microsoft.Fx.Portability.ObjectModel;
using System;
using System.Threading.Tasks;

namespace PortabilityService.AnalysisService.Controllers
{
    [Route("api/[controller]")]
    public class AnalyzeController : Controller
    {
        private readonly ILogger<AnalyzeController> _logger;
        private readonly IConfiguration _configuration;
        private readonly IRequestAnalyzer _requestAnalyzer;
        private readonly IStorage _storage;

        public AnalyzeController(
            IConfiguration configuration,
            IRequestAnalyzer requestAnalyzer,
            IStorage storage,
            ILogger<AnalyzeController> logger)
        {
            _configuration = configuration;
            _requestAnalyzer = requestAnalyzer;
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

                var result = AnalyzeRequest(request, submissionId);

                await _storage.SaveResultToBlobAsync(submissionId, result);

                return Ok();
            }
            catch (Exception exception)
            {
                _logger.LogError("Error occurs when analyzing request from submission '{submissionId}': {exception}", submissionId, exception);
                throw;
            }
        }

        private AnalyzeResult AnalyzeRequest(AnalyzeRequest analyzeRequest, string submissionId)
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

                return _requestAnalyzer.AnalyzeRequest(analyzeRequest, submissionId);
            }
        }
    }
}
