// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Fx.Portability;
using Microsoft.Fx.Portability.Cache;
using System;
using System.Linq;

namespace PortabilityService.AnalysisService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReadyController : ControllerBase
    {
        private readonly ApiPortData _apiPortData;
        private readonly IObjectCache<CatalogIndex> _catalog;

        public ReadyController(ApiPortData apiPortData, IObjectCache<CatalogIndex> catalog)
        {
            _apiPortData = apiPortData;
            _catalog = catalog;
        }

        [HttpGet]
        public IActionResult Ready() =>
            CatalogReady() && GitHubDataReady() ? Ok() : new StatusCodeResult(StatusCodes.Status503ServiceUnavailable);

        private bool CatalogReady() =>
            _catalog?.LastUpdated != default(DateTimeOffset) && _catalog.Value?.Catalog?.DocIds?.Count() > 0;

        private bool GitHubDataReady() =>
            _apiPortData?.BreakingChangesDictionary?.Keys.Count() > 0 && _apiPortData?.RecommendedChanges?.Count() > 0;
    }
}
