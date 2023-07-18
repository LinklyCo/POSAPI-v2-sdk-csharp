using System.Text.Json;
using System.Text.Json.Serialization;
using Linkly.PosApi.Sdk.Common;

namespace Linkly.PosApi.Sdk.UnitTest.Tests.Common;

public class JsonBoolToBitStringConverterTests
{
    [Theory]
    [InlineData("{\"Value\":\"1\"}", true)]
    [InlineData("{\"Value\":\"0\"}", false)]
    [InlineData("{\"Value\":null}", false)]
    [InlineData("{\"Value\":\"\"}", false)]
    [InlineData("{\"Value\":\"-1\"}", false)]
    [InlineData("{\"Value\":\"2\"}", false)]
    [InlineData("{\"Value\":\"00\"}", false)]
    public void Read_BitStringIsValid_SerialisedValueIsCorrect(string json, bool value)
    {
        var model = JsonSerializer.Deserialize<TestModel>(json);

        Assert.NotNull(model);
        Assert.Equal(value, model.Value);
    }

    [Theory]
    [InlineData(false, "{\"Value\":\"0\"}")]
    [InlineData(true, "{\"Value\":\"1\"}")]
    public void Write_Bool_SerialisesCorrectly(bool value, string expectedJson)
    {
        var actualJson = JsonSerializer.Serialize(new TestModel { Value = value });

        Assert.Equal(expectedJson, actualJson, StringComparer.Ordinal);
    }

    private class TestModel
    {
        [JsonConverter(typeof(JsonBoolToBitStringConverter))]
        public bool Value { get; set; }
    }
}