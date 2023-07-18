namespace Linkly.PosApi.Sdk.UnitTest.MockApi.Models;

internal class SettlementRequest : PosApiRequest
{
    public string Merchant { get; set; } = null!;

    public string Application { get; set; } = null!;

    public string SettlementType { get; set; } = null!;

    public bool? ResetTotals { get; set; }
}