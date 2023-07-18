using Linkly.PosApi.Sdk.Models;
using Linkly.PosApi.Sdk.Models.Transaction;
using Linkly.PosApi.Sdk.UnitTest.Common;

namespace Linkly.PosApi.Sdk.UnitTest.TestData;

internal sealed class RetrieveTransactionRequestNegativeTestDataBuilder : TestDataBuilder<RetrieveTransactionRequest>
{
    public RetrieveTransactionRequestNegativeTestDataBuilder() : base(() => new RetrieveTransactionRequest())
    {
        Add(req => req.ReferenceType, (ReferenceType)(-1));
        Add(req => req.ReferenceType, (ReferenceType)int.MaxValue);
        Add(req => req.Reference, null);
        Add(req => req.Reference, "");
    }
}