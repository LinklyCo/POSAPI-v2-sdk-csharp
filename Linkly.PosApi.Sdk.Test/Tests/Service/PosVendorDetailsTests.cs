using Linkly.PosApi.Sdk.Service;

namespace Linkly.PosApi.Sdk.UnitTest.Tests.Service;

public class PosVendorDetailsTests
{
    [Theory]
    [InlineData(null, "1.0.1", "posName")]
    [InlineData("", "1.0.1", "posName")]
    [InlineData(" ", "1.0.1", "posName")]
    [InlineData("test", null, "posVersion")]
    [InlineData("test", "", "posVersion")]
    [InlineData("test", " ", "posVersion")]
    public void Initialise_InvalidUris_ThrowsArgumentException(string? posName, string? posVersion, string invalidParam)
    {
        var ex = Assert.Throws<ArgumentException>(() => new PosVendorDetails(posName!, posVersion!, Guid.NewGuid(), Guid.NewGuid()));

        Assert.Equal(invalidParam, ex.ParamName);
    }
}