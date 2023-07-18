using System;

namespace Linkly.PosApi.Sdk.Models.Status
{
    /// <summary>Response to a <see cref="StatusRequest" />.</summary>
    public class StatusResponse : PosApiResponseWithResult
    {
        /// <summary>Two digit merchant code</summary>
        /// <example>00</example>
        public string Merchant { get; set; } = null!;

        /// <summary>The AIIC that is configured in the terminal.</summary>
        /// <example>444777</example>
        public string? Aiic { get; set; }

        /// <summary>The NII that is configured in the terminal.</summary>
        /// <example>0</example>
        public int NII { get; set; }

        /// <summary>Terminal ID configured in the PIN pad.</summary>
        /// <example>12345678</example>
        public string CatId { get; set; } = null!;

        /// <summary>Merchant ID configured in the PIN pad.</summary>
        /// <example>0123456789ABCDEF</example>
        public string CaId { get; set; } = null!;

        /// <summary>The bank response timeout that is configured in the terminal.</summary>
        /// <example>45</example>
        public int Timeout { get; set; }

        /// <summary>Indicates if the PIN pad is currently logged on.</summary>
        public bool LoggedOn { get; set; }

        /// <summary>The serial number of the terminal.</summary>
        /// <example>1234567890ABCDEF</example>
        public string PinPadSerialNumber { get; set; } = null!;

        /// <summary>PIN pad software version.</summary>
        /// <example>100800          </example>
        public string PinPadVersion { get; set; } = null!;

        /// <summary>The bank acquirer code.</summary>
        /// <example> </example>
        public string BankCode { get; set; } = null!;

        /// <summary>The bank description.</summary>
        /// <example> </example>
        public string BankDescription { get; set; } = null!;

        /// <summary>Key verification code.</summary>
        public string KVC { get; set; } = null!;

        /// <summary>Current number of stored transactions.</summary>
        public int SafCount { get; set; }

        /// <summary>PIN pad terminal network option.</summary>
        public NetworkType NetworkType { get; set; }

        /// <summary>The hardware serial number.</summary>
        /// <example>ABCDEF1234567890</example>
        public string HardwareSerial { get; set; } = null!;

        /// <summary>The merchant retailer name.</summary>
        /// <example>Example Retailer</example>
        public string RetailerName { get; set; } = null!;

        /// <summary>PIN pad terminal supported options flags.</summary>
        public PinPadOptionFlags OptionsFlags { get; set; } = new PinPadOptionFlags();

        /// <summary>Store-and forward credit limit.</summary>
        public int SafCreditLimit { get; set; }

        /// <summary>Store-and-forward debit limit.</summary>
        public int SafDebitLimit { get; set; }

        /// <summary>The maximum number of store transactions.</summary>
        public int MaxSaf { get; set; }

        /// <summary>The terminal key handling scheme.</summary>
        public KeyHandlingType KeyHandlingScheme { get; set; }

        /// <summary>The maximum cash out limit.</summary>
        public int CashoutLimit { get; set; }

        /// <summary>The maximum refund limit.</summary>
        public int RefundLimit { get; set; }

        /// <summary>Card prefix table version.</summary>
        public string CpatVersion { get; set; } = null!;

        /// <summary>Card name table version.</summary>
        public string NameTableVersion { get; set; } = null!;

        /// <summary>The terminal to PC communication type.</summary>
        public TerminalCommsType TerminalCommsType { get; set; }

        /// <summary>Number of card mis-reads.</summary>
        public int CardMisreadCount { get; set; }

        /// <summary>Number of memory pages in the PIN pad terminal.</summary>
        public int TotalMemoryInTerminal { get; set; }

        /// <summary>Number of free memory pages in the PIN pad terminal.</summary>
        public int FreeMemoryInTerminal { get; set; }

        /// <summary>The type of PIN pad terminal.</summary>
        public EftTerminalType EftTerminalType { get; set; }

        /// <summary>Number of applications in the terminal.</summary>
        public int NumAppsInTerminal { get; set; }

        /// <summary>Number of available display line on the terminal.</summary>
        public int NumLinesOnDisplay { get; set; }

        /// <summary>Hardware inception date.</summary>
        public DateTime HardwareInceptionDate { get; set; }
    }
}