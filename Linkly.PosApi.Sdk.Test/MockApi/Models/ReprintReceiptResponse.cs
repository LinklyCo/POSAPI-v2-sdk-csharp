namespace Linkly.PosApi.Sdk.UnitTest.MockApi.Models;

internal class ReprintReceiptResponse : PosApiResponse
{
    public string Merchant { get; set; } = null!;

    public List<string> ReceiptText { get; set; } = new();

    public bool Success { get; set; }

    public string ResponseCode { get; set; } = null!;

    public string ResponseText { get; set; } = null!;
}