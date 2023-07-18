namespace Linkly.PosApi.Sdk.UnitTest.MockApi.Models;

internal class SettlementResponse : PosApiResponse
{
    public string Merchant { get; set; } = null!;

    public string SettlementData { get; set; } = null!;

    public bool Success { get; set; }

    public string ResponseCode { get; set; } = null!;

    public string ResponseText { get; set; } = null!;
}