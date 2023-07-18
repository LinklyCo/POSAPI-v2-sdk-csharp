using System;

namespace Linkly.PosApi.Sdk.Service
{
    /// <summary>Authentication token and expiry.</summary>
    internal class AuthToken
    {
        public AuthToken(string token, DateTime expiry)
        {
            Token = token;
            Expiry = expiry;
        }

        /// <summary>Token required for bearer authentication in mock POS API requests.</summary>
        public string Token { get; set; }

        /// <summary>Expiry date of token.</summary>
        public DateTime Expiry { get; set; }

        /// <summary>
        /// Returns true if the token is due to expire in the next 5 minutes or if it has already
        /// expired.
        /// </summary>
        public bool IsExpiringSoon => DateTime.UtcNow > Expiry.AddMinutes(-5);
    }
}