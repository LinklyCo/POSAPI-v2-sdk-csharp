using System.Net;
using System.Text.Json;

namespace Linkly.PosApi.Sdk.Common
{
    /// <summary>Response object containing the HTTP response code and response body.</summary>
    internal class ApiResponse
    {
        /// <summary>Initialise a new response.</summary>
        /// <param name="success">Whether the response was successful.</param>
        /// <param name="statusCode">HTTP status code of the response.</param>
        /// <param name="body">Body of the response.</param>
        public ApiResponse(bool success, HttpStatusCode statusCode, string? body)
        {
            Success = success;
            StatusCode = statusCode;
            Body = body;
        }

        /// <summary>Whether the response was successful.</summary>
        public bool Success { get; set; }

        /// <summary>Status code of the response.</summary>
        public HttpStatusCode StatusCode { get; set; }

        /// <summary>Body of the response.</summary>
        public string? Body { get; set; }
    }

    /// <summary>
    /// Response object containing the HTTP response code, response body and deserialised message as
    /// <typeparamref name="TResponse" />.
    /// </summary>
    internal class JsonApiResponse<TResponse> : ApiResponse
        where TResponse : class
    {
        public JsonApiResponse(ApiResponse response, JsonSerializerOptions jsonSerializerOptions)
            : base(response.Success, response.StatusCode, response.Body)
        {
            if (Success && !string.IsNullOrEmpty(Body))
                Response = JsonSerializer.Deserialize<TResponse>(Body, jsonSerializerOptions);
        }

        /// <summary>HTTP response body deserialised into <typeparamref name="TResponse" />.</summary>
        public TResponse? Response { get; set; }

        /// <summary>Try and get the message of a successful API response.</summary>
        /// <param name="response">
        /// Outputs <paramref name="response" /> if the response was successful.
        /// Otherwise the output parameter is undefined.
        /// </param>
        /// <returns>True if <see cref="ApiResponse.Success" /> and <see cref="Response" /> is not null.</returns>
        public bool TryGetSuccessResponse(out TResponse response)
        {
            if (Success && !(Response is null))
            {
                response = Response;
                return true;
            }

            response = null!;
            return false;
        }
    }
}