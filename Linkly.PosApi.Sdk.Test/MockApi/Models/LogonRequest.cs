namespace Linkly.PosApi.Sdk.UnitTest.MockApi.Models;

internal class LogonRequest : PosApiRequest
{
    public string Merchant { get; set; } = null!;

    public string Application { get; set; } = null!;

    public string LogonType { get; set; } = null!;
}