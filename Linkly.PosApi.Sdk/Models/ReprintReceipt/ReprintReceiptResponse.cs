using System.Collections.ObjectModel;

namespace Linkly.PosApi.Sdk.Models.ReprintReceipt
{
    /// <summary>Response to a <see cref="ReprintReceiptRequest" />.</summary>
    public class ReprintReceiptResponse : PosApiResponseWithResult
    {
        /// <summary>Two digit merchant code</summary>
        /// <example>00</example>
        public string Merchant { get; set; } = null!;

        public Collection<string> ReceiptText { get; set; } = new Collection<string>();
    }
}