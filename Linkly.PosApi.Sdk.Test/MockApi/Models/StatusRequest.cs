namespace Linkly.PosApi.Sdk.UnitTest.MockApi.Models;

internal class StatusRequest : PosApiRequest
{
    public string Merchant { get; set; } = null!;

    public string Application { get; set; } = null!;

    public string StatusType { get; set; } = null!;
}