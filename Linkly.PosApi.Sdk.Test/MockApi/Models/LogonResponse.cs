namespace Linkly.PosApi.Sdk.UnitTest.MockApi.Models;

internal class LogonResponse : PosApiResponse
{
    public string PinPadVersion { get; set; } = null!;

    public bool Success { get; set; }

    public string ResponseCode { get; set; } = null!;

    public string ResponseText { get; set; } = null!;

    public DateTime Date { get; set; }

    public string CatId { get; set; } = null!;

    public string CaId { get; set; } = null!;

    public int Stan { get; set; }

    public Dictionary<string, string> PurchaseAnalysisData { get; set; } = new();
}