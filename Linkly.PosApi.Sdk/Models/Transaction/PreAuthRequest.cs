using System.Text.Json.Serialization;
using FluentValidation;
using FluentValidation.Results;
using Linkly.PosApi.Sdk.Common;

namespace Linkly.PosApi.Sdk.Models.Transaction
{
    /// <summary>
    /// Create a new pre-authorisation. Can also be used for account verification. See
    /// <see cref="Amount" />.
    /// </summary>
    public class PreAuthRequest : TransactionRequest
    {
        public override TxnType TxnType => TxnType.PreAuth;

        /// <summary>Pre-authorisation amount (in cents) or zero to perform an account verification.</summary>
        [JsonPropertyName("AmtPurchase")]
        public int Amount { get; set; }

        public override ValidationResult Validate() => new PreauthRequestValidator().Validate(this);
    }

    internal class PreauthRequestValidator : AbstractValidator<PreAuthRequest>
    {
        public PreauthRequestValidator()
        {
            Include(new TransactionRequestValidator());

            RuleFor(req => req.Amount).InclusiveBetween(0, Constants.Validation.MaxAmount);
        }
    }
}