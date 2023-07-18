namespace Linkly.PosApi.Sdk.UnitTest.MockApi;

/// <summary>Store session data related to a <see cref="MockApiHttpMessageHandler" /> instance.</summary>
internal class MockApiHttpMessageHandlerSession
{
    /// <summary>Permanent pair secret.</summary>
    public string? PairSecret { get; set; }

    /// <summary>Bearer token which needs to be refreshed before expiry.</summary>
    public List<AuthToken> AuthTokens { get; set; } = new();

    /// <summary>Store requests and responses associated with a mock API session.</summary>
    public RequestSessionManager Requests { get; set; } = new();

    /// <summary>Number of token authentication requests</summary>
    public int NumTokenRequests => AuthTokens.Count;
}