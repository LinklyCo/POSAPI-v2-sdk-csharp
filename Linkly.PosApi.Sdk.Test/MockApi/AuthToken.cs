namespace Linkly.PosApi.Sdk.UnitTest.MockApi;

/// <summary>Authentication token and expiry.</summary>
internal class AuthToken
{
    public AuthToken(string token, DateTime expiry)
    {
        Token = token;
        Expiry = expiry;
    }

    public string Token { get; set; }

    public DateTime Expiry { get; set; }
}