namespace Linkly.PosApi.Sdk.Models.Status
{
    /// <summary>PIN pad terminal supported options.</summary>
    public class PinPadOptionFlags
    {
        /// <summary>Tipping enabled flag.</summary>
        public bool Tipping { get; set; }

        /// <summary>Pre-authorisation enabled flag.</summary>
        public bool PreAuth { get; set; }

        /// <summary>Completions enabled flag.</summary>
        public bool Completions { get; set; }

        /// <summary>Cash-out enabled flag.</summary>
        public bool CashOut { get; set; }

        /// <summary>Refund enabled flag.</summary>
        public bool Refund { get; set; }

        /// <summary>Balance enquiry enabled flag.</summary>
        public bool Balance { get; set; }

        /// <summary>Deposit enabled flag.</summary>
        public bool Deposit { get; set; }

        /// <summary>Manual voucher enabled flag.</summary>
        public bool Voucher { get; set; }

        /// <summary>Mail-order/Telephone-order enabled flag.</summary>
        public bool Moto { get; set; }

        /// <summary>Auto-completions enabled flag.</summary>
        public bool AutoCompletion { get; set; }

        /// <summary>Electronic Fallback enabled flag.</summary>
        public bool EFB { get; set; }

        /// <summary>EMV enabled flag.</summary>
        public bool EMV { get; set; }

        /// <summary>Training mode enabled flag.</summary>
        public bool Training { get; set; }

        /// <summary>Withdrawal enabled flag.</summary>
        public bool Withdrawal { get; set; }

        /// <summary>Funds transfer enabled flag.</summary>
        public bool Transfer { get; set; }

        /// <summary>Start cash enabled flag.</summary>
        public bool StartCash { get; set; }
    }
}