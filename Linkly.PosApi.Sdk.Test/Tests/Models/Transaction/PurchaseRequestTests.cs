using Linkly.PosApi.Sdk.Common;
using Linkly.PosApi.Sdk.Models.Transaction;

namespace Linkly.PosApi.Sdk.UnitTest.Tests.Models.Transaction;

public class PurchaseRequestTests
{
    [Fact]
    public void SetTipOptions_VerifyPurchaseAnalysisData()
    {
        var request = new PurchaseRequest();

        request.SetTipOptions(new TippingOptions(5, 10, 15));

        Assert.Equal("[05,10,15]", request.PurchaseAnalysisData.GetValueOrDefault("TPO"));
    }

    [Fact]
    public void TipAmount_Set_VerifyPurchaseAnalysisData()
    {
        var request = new PurchaseRequest { TipAmount = 12345 };

        Assert.Equal("12345", request.PurchaseAnalysisData.GetValueOrDefault("TIP"));
    }

    [Fact]
    public void SetSurchargeOptions_VerifyPurchaseAnalysisData()
    {
        var request = new PurchaseRequest();
        var surchargeOptions = new SurchargeOptions();
        surchargeOptions.Add(new FixedSurcharge("4510", 158));
        surchargeOptions.Add(new PercentageSurcharge("22", 230));

        request.SetSurchargeOptions(surchargeOptions);

        Assert.Equal("[{\"b\":\"4510\",\"t\":\"$\",\"v\":158},{\"b\":\"22\",\"v\":230}]",
            request.PurchaseAnalysisData.GetValueOrDefault("SC2"));
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void ProductLevelBlock_Set_VerifyPurchaseAnalysisData(bool plb)
    {
        var request = new PurchaseRequest { ProductLevelBlock = plb };

        Assert.Equal(plb ? "1" : "0", request.PurchaseAnalysisData.GetValueOrDefault("PLB"));
    }
}