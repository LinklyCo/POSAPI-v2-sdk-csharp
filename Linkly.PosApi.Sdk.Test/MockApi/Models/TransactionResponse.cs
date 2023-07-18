namespace Linkly.PosApi.Sdk.UnitTest.MockApi.Models;

internal class TransactionResponse : PosApiResponse
{
    public string TxnType { get; set; } = null!;

    public string Merchant { get; set; } = null!;

    public string CardType { get; set; } = null!;

    public string CardName { get; set; } = null!;

    public string? RRN { get; set; }

    public DateTime DateSettlement { get; set; }

    public int AmtCash { get; set; }

    public int AmtPurchase { get; set; }

    public int AmtTip { get; set; }

    public int AuthCode { get; set; }

    public string TxnRef { get; set; } = null!;

    public string PAN { get; set; } = null!;

    public string DateExpiry { get; set; } = null!;

    public string Track2 { get; set; } = null!;

    public string AccountType { get; set; } = null!;

    public TxnFlags TxnFlags { get; set; } = new();

    public bool? BalanceReceived { get; set; }

    public int? AvailableBalance { get; set; }

    public int? ClearedFundsBalance { get; set; }

    public bool Success { get; set; }

    public string ResponseCode { get; set; } = null!;

    public string ResponseText { get; set; } = null!;

    public DateTime Date { get; set; }

    public string CatId { get; set; } = null!;

    public string CaId { get; set; } = null!;

    public int Stan { get; set; }

    public Dictionary<string, string> PurchaseAnalysisData { get; set; } = null!;

    public List<ReceiptResponse> Receipts { get; set; } = null!;
}