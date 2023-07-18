namespace Linkly.PosApi.Sdk.UnitTest.MockApi.Models;

internal class QueryCardResponse : PosApiResponse
{
    public string Merchant { get; set; } = null!;

    public bool IsTrack1Available { get; set; }

    public bool IsTrack2Available { get; set; }

    public bool IsTrack3Available { get; set; }

    public string Track1 { get; set; } = null!;

    public string Track2 { get; set; } = null!;

    public string Track3 { get; set; } = null!;

    public string CardName { get; set; } = null!;

    public string AccountType { get; set; } = null!;

    public bool Success { get; set; }

    public string ResponseCode { get; set; } = null!;

    public string ResponseText { get; set; } = null!;

    public Dictionary<string, string> PurchaseAnalysisData { get; set; } = new();
}