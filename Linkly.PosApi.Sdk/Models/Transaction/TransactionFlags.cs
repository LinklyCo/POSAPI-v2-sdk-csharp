namespace Linkly.PosApi.Sdk.Models.Transaction
{
    /// <summary>The flags that apply to the transaction.</summary>
    public class TransactionFlags
    {
        public string Offline { get; set; } = null!;

        public string ReceiptPrinted { get; set; } = null!;

        public CardEntryType CardEntry { get; set; }

        public CommsMethodType CommsMethod { get; set; }

        public CurrencyStatus Currency { get; set; }

        public PayPassStatus PayPass { get; set; }

        public string UndefinedFlag6 { get; set; } = null!;

        public string UndefinedFlag7 { get; set; } = null!;
    }
}