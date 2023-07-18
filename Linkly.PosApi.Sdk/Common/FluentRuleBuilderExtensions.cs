using System.Collections.Generic;
using System.Text.RegularExpressions;
using FluentValidation;

namespace Linkly.PosApi.Sdk.Common
{
    internal static class FluentRuleBuilderExtensions
    {
        private static readonly Regex CardExpiryRegex
            = new Regex("^(0[1-9]|1[0-2])[0-9][0-9]$", RegexOptions.Compiled, Constants.RegexTimeout);

        public static IRuleBuilderOptions<T, string?> CardExpiryDate<T>(this IRuleBuilder<T, string?> ruleBuilder) =>
            ruleBuilder
                .Matches(CardExpiryRegex)
                .WithMessage("'{PropertyName}' must be in format 'MMYY', You entered '{PropertyValue}'.");

        public static IRuleBuilderOptions<T, IDictionary<string, string>> HasValue<T>(
            this IRuleBuilder<T, IDictionary<string, string>> ruleBuilder, string key) =>
            ruleBuilder
                .Must(pad => !string.IsNullOrEmpty(pad.GetValueOrDefault(key)))
                .WithMessage($"'{key}' key must not be empty in the dictionary.");
    }
}