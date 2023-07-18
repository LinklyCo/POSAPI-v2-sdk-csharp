namespace Linkly.PosApi.Sdk.UnitTest.MockApi.Models;

internal class TransactionRequest : PosApiRequest
{
    public string Merchant { get; set; } = null!;

    public string Application { get; set; } = null!;

    public string TxnType { get; set; } = null!;

    public string CurrencyCode { get; set; } = null!;

    public string OriginalTxnType { get; set; } = null!;

    public DateTime? Date { get; set; }

    public DateTime? Time { get; set; }

    public bool? TrainingMode { get; set; }

    public bool? EnableTip { get; set; }

    public int? AmtCash { get; set; }

    public int? AmtPurchase { get; set; }

    public int? AuthCode { get; set; }

    public string TxnRef { get; set; } = null!;

    public string PanSource { get; set; } = null!;

    public string? PAN { get; set; } = null!;

    public string? DateExpiry { get; set; } = null!;

    public string? Track2 { get; set; } = null!;

    public string AccountType { get; set; } = null!;

    public string? RRN { get; set; }

    public int? CVV { get; set; }

    public Dictionary<string, string>? Basket { get; set; }
}