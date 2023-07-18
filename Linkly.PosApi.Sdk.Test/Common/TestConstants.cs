using Linkly.PosApi.Sdk.Service;
using Linkly.PosApi.Sdk.UnitTest.MockApi;

namespace Linkly.PosApi.Sdk.UnitTest.Common;

internal static class TestConstants
{
    public const int ExceededAmount = 1000000000;
    public static readonly PosVendorDetails PosVendorDetails = new("UnitTest POS", "1.0.1", Guid.NewGuid(), Guid.NewGuid());
    public static readonly ApiServiceEndpoint ServiceEndpoint = new(new Uri("https://authtest.com"), new Uri("https://postest.com"));
    public static readonly PairingCredentials PairingCreds = new("TestUser", "Password123!@#$", "135792");
}