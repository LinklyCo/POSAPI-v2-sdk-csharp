namespace Linkly.PosApi.Sdk.Models.Authentication
{
    /// <summary>Response to a <see cref="TokenRequest" />. Used internally for automatic token generation</summary>
    internal class TokenResponse : IBaseResponse
    {
        /// <summary>Transient token used for future requests to the PIN pad.</summary>
        public string Token { get; set; } = null!;

        /// <summary>Time (in seconds) the token is valid for.</summary>
        public long ExpirySeconds { get; set; }
    }
}