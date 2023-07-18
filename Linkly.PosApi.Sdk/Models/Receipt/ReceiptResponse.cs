using System.Collections.ObjectModel;
using System.Text.Json.Serialization;
using Linkly.PosApi.Sdk.Service;

namespace Linkly.PosApi.Sdk.Models.Receipt
{
    /// <summary>
    /// This message is returned asynchronously when a receipt event occurs on the PIN pad. To handle
    /// receipt events refer to <see cref="IPosApiEventListener.Receipt" />.
    /// </summary>
    public class ReceiptResponse : PosApiResponse
    {
        /// <summary>The receipt type.</summary>
        [JsonPropertyName("Type")]
        public ReceiptType ReceiptType { get; set; }

        /// <summary>Receipt text to be printed.</summary>
        public Collection<string> ReceiptText { get; set; } = new Collection<string>();

        /// <summary>Receipt response is a pre-print.</summary>
        public bool IsPrePrint { get; set; }
    }
}