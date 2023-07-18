using System.Net.Http.Headers;
using Linkly.PosApi.Sdk.Service;

namespace Linkly.PosApi.Sdk.Common
{
    internal static class AuthTokenExtensions
    {
        /// <summary>Return the authorisation header for an <see cref="AuthToken" />.</summary>
        public static AuthenticationHeaderValue GetAuthenticationHeaderValue(this AuthToken authToken) =>
            new AuthenticationHeaderValue("Bearer", authToken.Token);
    }
}