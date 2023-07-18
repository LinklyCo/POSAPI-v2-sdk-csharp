using Linkly.PosApi.Sdk.Models;
using Linkly.PosApi.Sdk.Models.Settlement;
using Linkly.PosApi.Sdk.UnitTest.Common;

namespace Linkly.PosApi.Sdk.UnitTest.TestData;

internal sealed class SettlementRequestNegativeTestDataBuilder : TestDataBuilder<SettlementRequest>
{
    public SettlementRequestNegativeTestDataBuilder() : base(RequestRepository.GetSettlementRequest)
    {
        Include<PosApiRequestNegativeTestDataBuilder>();

        Add(req => req.SettlementType, (SettlementType)(-1));
        Add(req => req.SettlementType, (SettlementType)int.MaxValue);
    }
}