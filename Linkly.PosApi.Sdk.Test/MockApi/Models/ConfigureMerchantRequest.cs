namespace Linkly.PosApi.Sdk.UnitTest.MockApi.Models;

internal class ConfigureMerchantRequest : PosApiRequest
{
    public string Merchant { get; set; } = null!;

    public string CatId { get; set; } = null!;

    public string CaId { get; set; } = null!;

    public string Application { get; set; } = null!;
}