namespace Linkly.PosApi.Sdk.UnitTest.MockApi.Models;

internal class PairingResponse : IBaseResponse
{
    public string Secret { get; set; } = null!;
}