using System.Collections.Generic;

namespace Linkly.PosApi.Sdk.Models
{
    /// <summary>Common model for POS API responses.</summary>
    public class PosApiResponse : IBaseResponse
    {
        /// <summary>Type of API response.</summary>
        public ResponseType ResponseType { get; set; }

        /// <summary>Additional data to be sent or received directly from the PIN pad.</summary>
        public IDictionary<string, string> PurchaseAnalysisData { get; set; } = new Dictionary<string, string>();
    }

    /// <summary>Response type for requests which have a response code.</summary>
    public abstract class PosApiResponseWithResult : PosApiResponse
    {
        /// <summary>Indicates if the request was successful.</summary>
        public bool Success { get; set; }

        /// <summary>
        /// The response code of the request. Will vary depending on the type of request. In most cases you can
        /// ignore this value and use <see cref="Success" /> in your business logic.
        /// </summary>
        /// <example>00</example>
        public string ResponseCode { get; set; } = null!;

        /// <summary>
        /// The response text for the response code. Will vary depending on the type of request. For some
        /// responses it may be appropriate to display this directly to the POS user.
        /// </summary>
        public string ResponseText { get; set; } = null!;
    }
}