using Linkly.PosApi.Sdk.Models;
using Linkly.PosApi.Sdk.UnitTest.Common;

namespace Linkly.PosApi.Sdk.UnitTest.TestData;

internal sealed class PosApiRequestNegativeTestDataBuilder : TestDataBuilder<PosApiRequest>
{
    public PosApiRequestNegativeTestDataBuilder(Func<PosApiRequest> getDefaultTestData) : base(getDefaultTestData)
    {
        Add(req => req.Merchant, null!);
        Add(req => req.Merchant, "0");
        Add(req => req.Merchant, "000");
        Add(req => req.Application, null!);
        Add(req => req.Application, "0");
        Add(req => req.Application, "000");
        Add(req => req.ReceiptAutoPrint, (ReceiptAutoPrint)(-1));
        Add(req => req.ReceiptAutoPrint, (ReceiptAutoPrint)int.MaxValue);
    }
}