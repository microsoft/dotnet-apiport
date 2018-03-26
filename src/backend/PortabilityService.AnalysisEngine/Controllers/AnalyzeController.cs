// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Fx.Portability.Analyzer;
using Microsoft.Fx.Portability.Azure.Storage;
using Microsoft.Fx.Portability.ObjectModel;
using Microsoft.WindowsAzure.Storage;

namespace PortabilityService.AnalysisEngine.Controllers
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
            this._configuration = configuration;
            this._requestAnalyzer = requestAnalyzer;
            this._storage = storage;
            this._logger = logger;
        }

        [HttpGet("{submissionId}")]
        public async Task<AnalyzeResponse> Get(string submissionId)
        {
            try
            {
                //TODO: replace with configuration service
                var connectionString = _configuration["BlobStorageConnectionString"];
                //TODO: get from DI
                var storage = new AzureStorage(CloudStorageAccount.Parse(connectionString));
                var request = await _storage.RetrieveRequestAsync(submissionId);

                // if the user opted out of us collecting telemetry
                if (!request.RequestFlags.HasFlag(AnalyzeRequestFlags.NoTelemetry))
                {
                    //TODO: remove the blob from Azure Blob Storage
                }

                return AnalyzeRequestAsync(request, submissionId);
            }
            catch (Exception exception)
            {
                _logger.LogError("Error occurs when analyzing request from {submissionId}: {exception}", submissionId, exception);
                return null;
            }
        }

        private AnalyzeResponse AnalyzeRequestAsync(AnalyzeRequest analyzeRequest, string submissionId)
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
