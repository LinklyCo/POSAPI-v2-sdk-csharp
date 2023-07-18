namespace Linkly.PosApi.Sdk.UnitTest.MockApi.Models;

internal class TokenRequest : IBaseRequest
{
    public string Secret { get; set; } = null!;

    public string PosName { get; set; } = null!;

    public string PosVersion { get; set; } = null!;

    public Guid PosId { get; set; }

    public Guid PosVendorId { get; set; }
}