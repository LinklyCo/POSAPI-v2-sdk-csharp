using Linkly.PosApi.Sdk.Common;
using Linkly.PosApi.Sdk.Service;

namespace Linkly.PosApi.Sdk.UnitTest.Tests.Common;

public class AuthTokenExtensionsTests
{
    [Fact]
    public void GetAuthenticationHeaderValue_TokenAsExpected()
    {
        var tokenValue = "Token";
        var token = new AuthToken(tokenValue, DateTime.UtcNow);

        var header = token.GetAuthenticationHeaderValue();

        Assert.Equal("Bearer", header.Scheme);
        Assert.Equal(tokenValue, header.Parameter);
    }
}