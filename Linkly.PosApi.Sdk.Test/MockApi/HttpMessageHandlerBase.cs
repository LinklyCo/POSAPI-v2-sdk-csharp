using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Linkly.PosApi.Sdk.UnitTest.MockApi;

/// <summary>Base class behaviour for creating a mock <see cref="HttpMessageHandler" />.</summary>
internal abstract class HttpMessageHandlerBase : HttpMessageHandler
{
    private const HttpStatusCode HttpStatusTooEarly = (HttpStatusCode)425;

    private readonly JsonSerializerOptions _jsonSerializerOptions;

    protected HttpMessageHandlerBase(JsonSerializerOptions? jsonSerializerOptions = null) =>
        _jsonSerializerOptions = jsonSerializerOptions ?? new JsonSerializerOptions
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

    /// <summary>
    /// Deserialise the content of <see cref="httpRequest" /> into
    /// <typeparamref name="TRequest" />.
    /// </summary>
    protected TRequest DeserialiseAsync<TRequest>(HttpRequestMessage httpRequest)
    {
        if (httpRequest.Content is null)
            throw new InvalidOperationException();

        var requestDto = httpRequest.Content.ReadFromJsonAsync<TRequest>(_jsonSerializerOptions).Result;
        return requestDto ?? throw new InvalidOperationException();
    }

    protected static HttpResponseMessage Ok() => new(HttpStatusCode.OK);

    /// <summary>
    /// Return a <see cref="HttpResponseMessage" /> with an <see cref="HttpStatusCode.OK" /> status code
    /// and a <see cref="response" /> object serialised to json in the response body.
    /// </summary>
    /// <typeparam name="TResponse"></typeparam>
    /// <param name="response">Response to serialise into the body.</param>
    protected HttpResponseMessage Ok<TResponse>(TResponse? response)
    {
        var httpResponse = Ok();
        if (response is not null)
            httpResponse.Content = JsonContent.Create(response, options: _jsonSerializerOptions);

        return httpResponse;
    }

    protected static HttpResponseMessage TooEarly() => new(HttpStatusTooEarly);

    protected static HttpResponseMessage Unauthorized() => new(HttpStatusCode.Unauthorized);

    protected static HttpResponseMessage NotFound() => new(HttpStatusCode.NotFound);

    protected static HttpResponseMessage BadRequest() => new(HttpStatusCode.BadRequest);
}