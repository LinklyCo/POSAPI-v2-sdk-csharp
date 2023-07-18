using Linkly.PosApi.Sdk.Models.Result;
using Linkly.PosApi.Sdk.UnitTest.Common;

namespace Linkly.PosApi.Sdk.UnitTest.TestData;

internal sealed class ResultRequestNegativeTestDataBuilder : TestDataBuilder<ResultRequest>
{
    public ResultRequestNegativeTestDataBuilder() : base(() => RequestRepository.GetResultRequest(Guid.NewGuid()))
    {
        Add(req => req.SessionId, Guid.Empty);
    }
}