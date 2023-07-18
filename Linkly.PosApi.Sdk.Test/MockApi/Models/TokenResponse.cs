namespace Linkly.PosApi.Sdk.UnitTest.MockApi.Models;

internal class TokenResponse : IBaseResponse
{
    public string Token { get; set; } = null!;

    public long ExpirySeconds { get; set; }
}