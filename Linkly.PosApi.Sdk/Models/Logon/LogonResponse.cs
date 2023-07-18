using System;

namespace Linkly.PosApi.Sdk.Models.Logon
{
    /// <summary>Response to a <see cref="LogonRequest" />.</summary>
    public class LogonResponse : PosApiResponseWithResult
    {
        /// <summary>PIN pad software version.</summary>
        /// <example>100800          </example>
        public string PinPadVersion { get; set; } = null!;

        /// <summary>Date and time of the response returned by the bank.</summary>
        public DateTime Date { get; set; }

        /// <summary>Terminal ID configured in the PIN pad.</summary>
        /// <example>12345678</example>
        public string CatId { get; set; } = null!;

        /// <summary>Merchant ID configured in the PIN pad.</summary>
        /// <example>0123456789ABCDEF</example>
        public string CaId { get; set; } = null!;

        /// <summary>System Trace Audit Number</summary>
        public int Stan { get; set; }
    }
}