namespace Linkly.PosApi.Sdk.UnitTest.MockApi.Models;

internal class ReceiptResponse : PosApiResponse
{
    public string Type { get; set; } = null!;

    public List<string> ReceiptText { get; set; } = new();

    public bool IsPrePrint { get; set; }
}