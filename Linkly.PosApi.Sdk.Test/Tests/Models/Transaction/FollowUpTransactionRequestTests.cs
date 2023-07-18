using FluentValidation.Results;
using Linkly.PosApi.Sdk.Models;
using Linkly.PosApi.Sdk.Models.Transaction;

namespace Linkly.PosApi.Sdk.UnitTest.Tests.Models.Transaction;

public class FollowUpTransactionRequestTests
{
    [Fact]
    public void Rfn_Set_CheckPurchaseAnalysisData()
    {
        const string rfn = "123";
        var request = new TestFollowUpTransaactionRequest { RFN = rfn };

        Assert.Equal(rfn, request.PurchaseAnalysisData["RFN"]);
    }

    private class TestFollowUpTransaactionRequest : FollowUpTransactionRequest
    {
        public override TxnType TxnType => TxnType.NotSet;
        public override ValidationResult Validate() => default!;
    }
}