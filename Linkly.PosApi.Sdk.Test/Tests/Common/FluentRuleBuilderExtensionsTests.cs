using FluentValidation;
using FluentValidation.TestHelper;
using Linkly.PosApi.Sdk.Common;

namespace Linkly.PosApi.Sdk.UnitTest.Tests.Common;

public class FluentRuleBuilderExtensionsTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("0100")]
    [InlineData("0199")]
    [InlineData("1200")]
    [InlineData("1299")]
    public void CardExpiryDate_ValidExpiry_ValidationSuccess(string expiry)
    {
        var validator = new CardExpiryTestValidator();
        validator.TestValidate(new Test { Expiry = expiry }).ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData("010")]
    [InlineData("0000")]
    [InlineData("1300")]
    [InlineData("01000")]
    [InlineData("ABCD")]
    [InlineData("01-99")]
    [InlineData("01 99")]
    [InlineData("Ø¢«®?")]
    public void CardExpiryDate_InvalidExpiry_ValidationError(string expiry)
    {
        var validator = new CardExpiryTestValidator();
        validator.TestValidate(new Test { Expiry = expiry })
            .ShouldHaveValidationErrorFor(nameof(Test.Expiry))
            .WithErrorMessage($"'{nameof(Test.Expiry)}' must be in format 'MMYY', You entered '{expiry}'.");
    }

    [Theory]
    [InlineData(" ")]
    [InlineData("a")]
    public void HasValue_ValueNotEmpty_ValidationSuccess(string value)
    {
        var testData = new Test();
        testData.Dict.Add("Key", value);
        var validator = new HasValueTestValidator("Key");

        validator.TestValidate(testData).ShouldNotHaveValidationErrorFor(nameof(Test.Dict));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void HasValue_ValueEmpty_ValidationError(string? value)
    {
        var testData = new Test();
        testData.Dict.Add("Key", value!);
        var validator = new HasValueTestValidator("Key");

        validator.TestValidate(testData)
            .ShouldHaveValidationErrorFor(nameof(Test.Dict))
            .WithErrorMessage("'Key' key must not be empty in the dictionary.");
    }

    internal sealed class Test
    {
        public string? Expiry { get; init; }
        public IDictionary<string, string> Dict { get; init; } = new Dictionary<string, string>();
    }

    internal sealed class CardExpiryTestValidator : AbstractValidator<Test>
    {
        public CardExpiryTestValidator() => RuleFor(test => test.Expiry).CardExpiryDate();
    }

    internal sealed class HasValueTestValidator : AbstractValidator<Test>
    {
        public HasValueTestValidator(string key) => RuleFor(test => test.Dict).HasValue(key);
    }
}