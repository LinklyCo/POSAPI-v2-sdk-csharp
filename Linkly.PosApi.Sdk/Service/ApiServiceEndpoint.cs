using System;

namespace Linkly.PosApi.Sdk.Service
{
    /// <summary>Configuration for a <see cref="PosApiService" />.</summary>
    public class ApiServiceEndpoint
    {
        public readonly Uri AuthApiBaseUri;
        public readonly Uri PosApiBaseUri;

        /// <summary>
        /// Specify API endpoints for <see cref="PosApiService" />. This requires an endpoint for the POS
        /// service and Auth service.
        /// </summary>
        /// <param name="authApiBaseUri">
        /// Base URI of the Auth API service.
        /// Example: <example>https://auth.sandbox.cloud.pceftpos.com</example>
        /// </param>
        /// <param name="posApiBaseUri">
        /// Base URI of the POS API service.
        /// Example: <example>https://rest.pos.sandbox.cloud.pceftpos.com</example>
        /// </param>
        public ApiServiceEndpoint(Uri authApiBaseUri, Uri posApiBaseUri)
        {
            static bool IsBaseUri(Uri uri) =>
                uri.IsAbsoluteUri && string.Equals(uri.PathAndQuery, "/", StringComparison.Ordinal);

            PosApiBaseUri = IsBaseUri(posApiBaseUri)
                ? posApiBaseUri
                : throw new ArgumentException("URI must be absolute and not contain a path", nameof(posApiBaseUri));
            AuthApiBaseUri = IsBaseUri(authApiBaseUri)
                ? authApiBaseUri
                : throw new ArgumentException("URI must be absolute and not contain a path", nameof(authApiBaseUri));
        }
    }
}