// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace PortabilityService.WorkflowManagement
{
    abstract class BaseAction
    {
        protected static HttpClient httpClient = new HttpClient();

        public BaseAction(string serviceUrl)
        {
            ServiceUrl = serviceUrl;
        }

        public string ServiceUrl { get; private set; }
    }
}
