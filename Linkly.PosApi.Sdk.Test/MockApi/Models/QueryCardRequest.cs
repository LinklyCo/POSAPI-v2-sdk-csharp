namespace Linkly.PosApi.Sdk.UnitTest.MockApi.Models;

internal class QueryCardRequest : PosApiRequest
{
    public string Merchant { get; set; } = null!;

    public string QueryCardType { get; set; } = null!;

    public string Application { get; set; } = null!;
}