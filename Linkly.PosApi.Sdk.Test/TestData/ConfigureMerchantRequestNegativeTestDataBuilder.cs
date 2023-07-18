using Linkly.PosApi.Sdk.Models.ConfigureMerchant;
using Linkly.PosApi.Sdk.UnitTest.Common;

namespace Linkly.PosApi.Sdk.UnitTest.TestData;

internal sealed class ConfigureMerchantRequestNegativeTestDataBuilder : TestDataBuilder<ConfigureMerchantRequest>
{
    public ConfigureMerchantRequestNegativeTestDataBuilder() : base(RequestRepository.GetConfigureMerchantRequest)
    {
        Include<PosApiRequestNegativeTestDataBuilder>();

        Add(req => req.CatId, null);
        Add(req => req.CatId, "");
        Add(req => req.CaId, null);
        Add(req => req.CaId, "");
    }
}