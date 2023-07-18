using Linkly.PosApi.Sdk.Models;
using Linkly.PosApi.Sdk.Models.QueryCard;
using Linkly.PosApi.Sdk.UnitTest.Common;

namespace Linkly.PosApi.Sdk.UnitTest.TestData;

internal sealed class QueryCardRequestNegativeTestDataBuilder : TestDataBuilder<QueryCardRequest>
{
    public QueryCardRequestNegativeTestDataBuilder() : base(RequestRepository.GetQueryCardRequest)
    {
        Include<PosApiRequestNegativeTestDataBuilder>();

        Add(req => req.QueryCardType, (QueryCardType)(-1));
        Add(req => req.QueryCardType, (QueryCardType)int.MaxValue);
    }
}