namespace Linkly.PosApi.Sdk.UnitTest.MockApi.Models;

internal class IntermediateResponse : IBaseResponse
{
    /// <summary>
    /// Initialise a new intermediate response which should contain the request ID of the initial request
    /// and the complete URI of the response endpoint for the specific request.
    /// </summary>
    /// <param name="sessionId">Session ID of the initial request.</param>
    /// <param name="resultUriTemplate">
    /// Result URI template. Example: <example>/v2/sessions/{0}/result</example>
    /// </param>
    public IntermediateResponse(Guid sessionId, string resultUriTemplate)
    {
        RequestId = sessionId.ToString("D");
        Uri = string.Format(resultUriTemplate, RequestId);
    }

    /// <summary>Session ID of the request.</summary>
    public string RequestId { get; }

    /// <summary>Redirection URI to follow to fetch the events associated with the request.</summary>
    public string Uri { get; }
}