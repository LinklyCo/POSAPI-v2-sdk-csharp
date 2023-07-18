namespace Linkly.PosApi.Sdk.UnitTest.MockApi.Models;

internal class PosApiRequest : IBaseRequest
{
    public string ReceiptAutoPrint { get; set; } = null!;

    public string CutReceipt { get; set; } = null!;

    public IDictionary<string, string>? PurchaseAnalysisData { get; set; }

    public string PosName { get; set; } = null!;

    public string PosVersion { get; set; } = null!;

    public Guid PosId { get; set; }
}