using Linkly.PosApi.Sdk.Common;
using Linkly.PosApi.Sdk.Models.Transaction;
using Linkly.PosApi.Sdk.UnitTest.TestData;

namespace Linkly.PosApi.Sdk.UnitTest.Tests.Models.Transaction;

public class TippingOptionsTests
{
    [Theory]
    [ClassData(typeof(ValidTippingOptionsTestDataBuilder))]
    public void ToPadString_Initialise_VerifyStringRepresentation(TippingOptions? actual, string? expected)
    {
        Assert.Equal(expected, (actual as IPurchaseAnalysisDataConverter)?.ToPadString());
    }

    [Theory]
    [ClassData(typeof(InvalidTippingOptionsTestDataBuilder))]
    public void InitialiseWithInvalidTipPercentages_ThrowsArgumentException(Action createTippingOptions)
    {
        Assert.Throws<ArgumentException>(createTippingOptions);
    }
}