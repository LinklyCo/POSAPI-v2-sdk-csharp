using Linkly.PosApi.Sdk.Common;

namespace Linkly.PosApi.Sdk.UnitTest.Tests.Common;

public class DictionaryExtensionsTests
{
    private readonly IDictionary<string, string> _testDict;

    public DictionaryExtensionsTests() =>
        _testDict = new Dictionary<string, string>
        {
            ["MyKey"] = "MyValue"
        };

    [Fact]
    public void GetValueOrDefault_KeyDoesNotExist_ReturnsDefault()
    {
        Assert.Null(_testDict.GetValueOrDefault("Missing"));
    }

    [Fact]
    public void GetValueOrDefault_KeyDoesExist_ReturnsValue()
    {
        Assert.Equal("MyValue", _testDict.GetValueOrDefault("MyKey"));
    }
}