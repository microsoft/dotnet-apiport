// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Net.Http;

namespace Microsoft.Fx.Portability
{
    public class ServiceResponse<T>
    {
        public T Response { get; private set; }
        public ServiceHeaders Headers { get; private set; }

        public ServiceResponse(T response)
        {
            Response = response;
            Headers = new ServiceHeaders();
        }

        public ServiceResponse(T response, ServiceHeaders headers)
        {
            Response = response;
            Headers = headers;
        }

        public ServiceResponse(T response, EndpointStatus status)
        {
            Response = response;
            Headers = new ServiceHeaders { Status = status };
        }

        public ServiceResponse(T result, HttpResponseMessage response)
        {
            Response = result;
            Headers = new ServiceHeaders(response);
        }
    }

    public static class ServiceResponse
    {
        public static ServiceResponse<T> Create<T>(T response)
        {
            return new ServiceResponse<T>(response);
        }

        public static ServiceResponse<T> Create<T>(T response, ServiceHeaders headers)
        {
            return new ServiceResponse<T>(response, headers);
        }
    }
}
