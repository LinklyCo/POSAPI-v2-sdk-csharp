using Linkly.PosApi.Sdk.Models.Transaction;

namespace Linkly.PosApi.Sdk.UnitTest.Tests.Models.Transaction;

public class TransactionResponseTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("123")]
    public void Rfn_SetPurchaseAnalysisData_VerifyRfn(string? rfn)
    {
        var response = new TransactionResponse();
        if (rfn is not null)
            response.PurchaseAnalysisData["RFN"] = rfn;

        Assert.Equal(rfn, response.RFN);
    }

    [Theory]
    [InlineData(null, null)]
    [InlineData(1100, "00001100")]
    public void AmountTotal_SetPurchaseAnalysisData_VerifyAmountTotal(int? amount, string? amountTag)
    {
        var response = new TransactionResponse();
        if (amountTag is not null)
            response.PurchaseAnalysisData["AMT"] = amountTag;

        Assert.Equal(amount, response.AmountTotal);
    }

    [Theory]
    [InlineData(null, null)]
    [InlineData(11, "00000011")]
    public void AmountSurcharge_SetPurchaseAnalysisData_VerifyAmountSurcharge(int? surcharge, string? surchargeTag)
    {
        var response = new TransactionResponse();
        if (surchargeTag is not null)
            response.PurchaseAnalysisData["SUR"] = surchargeTag;

        Assert.Equal(surcharge, response.AmountSurcharge);
    }
}