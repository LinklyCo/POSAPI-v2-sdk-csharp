using Linkly.PosApi.Sdk.Common;
using Linkly.PosApi.Sdk.Models.Transaction;
using Linkly.PosApi.Sdk.UnitTest.TestData;

namespace Linkly.PosApi.Sdk.UnitTest.Tests.Models.Transaction;

public class SurchargeOptionsTests
{
    [Fact]
    public void ToPadString_InitialiseWithMultipleSurchargeRules_VerifyStringRepresentation()
    {
        var surchargeOptions = new SurchargeOptions();
        surchargeOptions.Add(new PercentageSurcharge("12", 1));
        surchargeOptions.Add(new PercentageSurcharge("34", 9999));
        surchargeOptions.Add(new FixedSurcharge("9875", 1));
        surchargeOptions.Add(new FixedSurcharge("5512", 999999999));

        Assert.Equal(
            @"[{""b"":""12"",""v"":1},{""b"":""34"",""v"":9999},{""b"":""9875"",""t"":""$"",""v"":1},{""b"":""5512"",""t"":""$"",""v"":999999999}]",
            ((IPurchaseAnalysisDataConverter)surchargeOptions).ToPadString());
    }

    [Fact]
    public void ToPadString_InitialiseWithoutSurchargeRules_VerifyStringRepresentation()
    {
        var surchargeOptions = new SurchargeOptions();
        Assert.Equal("[]", ((IPurchaseAnalysisDataConverter) surchargeOptions).ToPadString());
    }

    [Theory]
    [ClassData(typeof(InvalidSurchargeRulesTestDataBuilder))]
    public void InitialiseWithInvalidSurchargeRule_ThrowsArgumentException(Action createSurchargeRule, string invalidParam)
    {
        Assert.Throws<ArgumentException>(invalidParam, createSurchargeRule);
    }
}