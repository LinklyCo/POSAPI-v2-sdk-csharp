using Linkly.PosApi.Sdk.Models.Transaction;

namespace Linkly.PosApi.Sdk.UnitTest.TestData;

internal sealed class InvalidSurchargeRulesTestDataBuilder : TheoryData<Action, string>
{
    public InvalidSurchargeRulesTestDataBuilder()
    {
        Add(() => _ = new FixedSurcharge(null!, 1), "bin");
        Add(() => _ = new FixedSurcharge("", 1), "bin");
        Add(() => _ = new FixedSurcharge("1", 1), "bin");
        Add(() => _ = new FixedSurcharge("1234567", 1), "bin");
        Add(() => _ = new FixedSurcharge("123", 0), "amountInCents");
        Add(() => _ = new FixedSurcharge("123", 1000000000), "amountInCents");
        Add(() => _ = new PercentageSurcharge(null!, 1), "bin");
        Add(() => _ = new PercentageSurcharge("", 1), "bin");
        Add(() => _ = new PercentageSurcharge("1", 1), "bin");
        Add(() => _ = new PercentageSurcharge("1234567", 1), "bin");
        Add(() => _ = new PercentageSurcharge("123", 0), "basisPoints");
        Add(() => _ = new PercentageSurcharge("123", 10000), "basisPoints");
    }
}