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
}
