namespace Linkly.PosApi.Sdk.UnitTest.MockApi.Models;

internal class SendKeyRequest : PosApiRequest
{
    public string Key { get; set; } = null!;

    public string Data { get; set; } = null!;
}