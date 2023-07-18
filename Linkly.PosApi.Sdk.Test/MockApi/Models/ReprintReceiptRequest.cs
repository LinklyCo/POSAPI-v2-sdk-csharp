namespace Linkly.PosApi.Sdk.UnitTest.MockApi.Models;

internal class ReprintReceiptRequest : PosApiRequest
{
    public string Merchant { get; set; } = null!;

    public string Application { get; set; } = null!;

    public string ReprintType { get; set; } = null!;
}