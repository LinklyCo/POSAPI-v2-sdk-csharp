namespace Linkly.PosApi.Sdk.UnitTest.MockApi.Models;

internal class PairingRequest : IBaseRequest
{
    public string Username { get; set; } = null!;

    public string Password { get; set; } = null!;

    public string PairCode { get; set; } = null!;
}