namespace Microsoft.Fx.Portability.Proxy
{
    /// <summary>
    /// Taken from: https://github.com/NuGet/NuGet.Client/blob/dev/src/NuGet.Core/NuGet.Configuration/Credential/CredentialRequestType.cs
    /// </summary>
    public enum CredentialRequestType
    {
        /// <summary>
        /// Indicates that the request credentials are to be used to access a proxy.
        /// </summary>
        Proxy,

        /// <summary>
        /// Indicates that the remote server rejected the previous request as unauthorized. This 
        /// suggests that the server does not know who the caller is (i.e. the caller is not
        /// authenticated).
        /// </summary>
        Unauthorized,

        /// <summary>
        /// Indicates that the remote server rejected the previous request as forbidden. This
        /// suggests that the server knows who the caller is (i.e. the caller is authorized) but
        /// is not allowed to access the request resource. A different set of credentials could
        /// solve this failure.
        /// </summary>
        Forbidden
    }
}
