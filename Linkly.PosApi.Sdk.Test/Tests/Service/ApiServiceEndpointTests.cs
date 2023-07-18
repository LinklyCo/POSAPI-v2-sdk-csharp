using Linkly.PosApi.Sdk.Service;

namespace Linkly.PosApi.Sdk.UnitTest.Tests.Service;

public class ApiServiceEndpointTests
{
    public static IEnumerable<object[]> InvalidUriTestData => new[]
    {
        new object[] { new Uri("https://fake.com/api"), new Uri("https://fake.com"), "authApiBaseUri" },
        new object[] { new Uri("/foo/bar", UriKind.Relative), new Uri("https://fake.com"), "authApiBaseUri" },
        new object[] { new Uri("https://fake.com"), new Uri("https://fake.com/api"), "posApiBaseUri" },
        new object[] { new Uri("https://fake.com"), new Uri("/foo/bar", UriKind.Relative), "posApiBaseUri" }
    };

    [Theory]
    [MemberData(nameof(InvalidUriTestData))]
    public void Initialise_InvalidUris_ThrowsArgumentException(Uri authApiDomain, Uri posApiDomain, string invalidParam)
    {
        var ex = Assert.Throws<ArgumentException>(() => new ApiServiceEndpoint(authApiDomain, posApiDomain));

        Assert.Equal(invalidParam, ex.ParamName);
    }
}