using Linkly.PosApi.Sdk.Service;

namespace Linkly.PosApi.Sdk.UnitTest.Tests.Service;

public class AuthTokenTests
{
    [Theory]
    [InlineData(-5, true)]
    [InlineData(4, true)]
    [InlineData(6, false)]
    public void IsExpiringSoon_Tests(int expiryMinutes, bool isExpiringSoon)
    {
        var authToken = new AuthToken("123", DateTime.UtcNow.AddMinutes(expiryMinutes));

        Assert.Equal(isExpiringSoon, authToken.IsExpiringSoon);
    }
}