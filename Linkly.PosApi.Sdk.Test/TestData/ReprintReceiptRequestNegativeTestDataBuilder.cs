using Linkly.PosApi.Sdk.Models;
using Linkly.PosApi.Sdk.Models.ReprintReceipt;
using Linkly.PosApi.Sdk.UnitTest.Common;

namespace Linkly.PosApi.Sdk.UnitTest.TestData;

internal sealed class ReprintReceiptRequestNegativeTestDataBuilder : TestDataBuilder<ReprintReceiptRequest>
{
    public ReprintReceiptRequestNegativeTestDataBuilder() : base(RequestRepository.GetReprintReceiptRequest)
    {
        Include<PosApiRequestNegativeTestDataBuilder>();

        Add(req => req.ReprintType, (ReprintType)(-1));
        Add(req => req.ReprintType, (ReprintType)int.MaxValue);
    }
}