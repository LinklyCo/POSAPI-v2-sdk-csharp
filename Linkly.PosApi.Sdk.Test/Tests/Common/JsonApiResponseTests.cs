using System.Net;
using System.Text.Json;
using Linkly.PosApi.Sdk.Common;

namespace Linkly.PosApi.Sdk.UnitTest.Tests.Common;

public class JsonApiResponseTests
{
    [Fact]
    public void SuccessfulResponseAndValidBody_ValidateMethodsAndProperties()
    {
        const string body = "{\"Name\": \"Bar\"}";

        var response = new JsonApiResponse<Foo>(new ApiResponse(true, HttpStatusCode.OK, body), new JsonSerializerOptions());

        Assert.True(response.Success);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(body, response.Body);
        Assert.NotNull(response.Response);
        Assert.Equal("Bar", response.Response.Name);
        Assert.True(response.TryGetSuccessResponse(out var message));
        Assert.Equal("Bar", message.Name);
    }

    [Fact]
    public void UnsuccessfulResponseAndValidBody_ValidateMethodsAndProperties()
    {
        const string body = "{\"Name\": \"Bar\"}";

        var response = new JsonApiResponse<Foo>(new ApiResponse(false, HttpStatusCode.NotFound, body), new JsonSerializerOptions());

        Assert.False(response.Success);
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Equal(body, response.Body);
        Assert.Null(response.Response);
        Assert.False(response.TryGetSuccessResponse(out var message));
        Assert.Null(message);
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void SuccessfulResponseAndInvalidBody_ValidateMethodsAndProperties(string body)
    {
        var response = new JsonApiResponse<Foo>(new ApiResponse(true, HttpStatusCode.OK, body), new JsonSerializerOptions());

        Assert.True(response.Success);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(body, response.Body);
        Assert.Null(response.Response);
        Assert.False(response.TryGetSuccessResponse(out var message));
        Assert.Null(message);
    }

    [Fact]
    public void SuccessfulResponseAndInvalidBody_ThrowsJsonException()
    {
        Assert.Throws<JsonException>(() =>
            new JsonApiResponse<Foo>(new ApiResponse(true, HttpStatusCode.OK, "{\"Bar\"}"), new JsonSerializerOptions()));
    }

    private class Foo
    {
        public string Name { get; set; } = null!;
    }
}