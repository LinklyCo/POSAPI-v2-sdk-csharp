using Linkly.PosApi.Sdk.Common;
using Linkly.PosApi.Sdk.Models.Transaction;

namespace Linkly.PosApi.Sdk.UnitTest.Tests.Models.Transaction;

public class PreAuthSummaryRequestTests
{
    [Fact]
    public void PreAuthIndex_Set_CheckPurchaseAnalysisData()
    {
        var request = new PreAuthSummaryRequest {PreAuthIndex = 5};

        Assert.Equal("5", request.PurchaseAnalysisData.GetValueOrDefault("PAI"));
    }
}