using System;
using System.Net;

namespace Linkly.PosApi.Sdk.Models
{
    /// <summary>Error response handler</summary>
    public class ErrorResponse
    {
        public ErrorResponse(ErrorSource source, string? message, HttpStatusCode? httpStatusCode, Exception? exception = null)
        {
            Source = source;
            HttpStatusCode = httpStatusCode;
            Message = message;
            Exception = exception;
        }

        /// <summary>Source of the error.</summary>
        public ErrorSource Source { get; set; }

        /// <summary>HTTP error code of API response. Will only be present if <see cref="ErrorSource.API" />.</summary>
        public HttpStatusCode? HttpStatusCode { get; set; }

        /// <summary>Error message.</summary>
        public string? Message { get; set; }

        /// <summary>Exception raised in SDK. Will only be present if <see cref="ErrorSource.Internal" />.</summary>
        public Exception? Exception { get; set; }
    }
}