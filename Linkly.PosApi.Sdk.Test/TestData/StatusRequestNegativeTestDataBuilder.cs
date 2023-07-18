using Linkly.PosApi.Sdk.Models;
using Linkly.PosApi.Sdk.Models.Status;
using Linkly.PosApi.Sdk.UnitTest.Common;

namespace Linkly.PosApi.Sdk.UnitTest.TestData;

internal sealed class StatusRequestNegativeTestDataBuilder : TestDataBuilder<StatusRequest>
{
    public StatusRequestNegativeTestDataBuilder() : base(RequestRepository.GetStatusRequest)
    {
        Include<PosApiRequestNegativeTestDataBuilder>();

        Add(req => req.StatusType, (StatusType)(-1));
        Add(req => req.StatusType, (StatusType)int.MaxValue);
    }
}