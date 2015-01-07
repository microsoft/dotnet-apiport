using System;
using System.Threading.Tasks;

namespace Microsoft.Fx.Portability
{
    public interface IAuthenticationService
    {
        Task<string> GetAccessTokenAsync();
        Task<DateTimeOffset> GetExpirationAsync();
    }
}
