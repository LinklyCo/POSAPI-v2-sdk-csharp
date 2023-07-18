using Linkly.PosApi.Sdk.Models.Transaction;

namespace Linkly.PosApi.Sdk.UnitTest.TestData;

internal sealed class ValidTippingOptionsTestDataBuilder : TheoryData<TippingOptions?, string?>
{
    public ValidTippingOptionsTestDataBuilder()
    {
        Add(null, null);
        Add(new TippingOptions(1), "[01]");
        Add(new TippingOptions(1, 2), "[01,02]");
        Add(new TippingOptions(1, 2, 3), "[01,02,03]");
        Add(new TippingOptions(10, 15, 20), "[10,15,20]");
    }
}

internal sealed class InvalidTippingOptionsTestDataBuilder : TheoryData<Action>
{
    public InvalidTippingOptionsTestDataBuilder()
    {
        Add(() => _ = new TippingOptions());
        Add(() => _ = new TippingOptions(1, 1, 1, 1));
        Add(() => _ = new TippingOptions(0));
        Add(() => _ = new TippingOptions(0, 1, 1));
        Add(() => _ = new TippingOptions(100));
        Add(() => _ = new TippingOptions(100, 1, 1));
        Add(() => _ = new TippingOptions(1, 0));
        Add(() => _ = new TippingOptions(1, 0, 1));
        Add(() => _ = new TippingOptions(1, 100));
        Add(() => _ = new TippingOptions(1, 100, 1));
        Add(() => _ = new TippingOptions(1, 1, 0));
        Add(() => _ = new TippingOptions(1, 1, 100));
    }
}