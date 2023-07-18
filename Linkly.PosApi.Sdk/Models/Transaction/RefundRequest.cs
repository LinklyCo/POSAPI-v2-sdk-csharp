using System.Text.Json.Serialization;
using FluentValidation;
using FluentValidation.Results;
using Linkly.PosApi.Sdk.Common;

namespace Linkly.PosApi.Sdk.Models.Transaction
{
    /// <summary>Refund transaction.</summary>
    public class RefundRequest : FollowUpTransactionRequest
    {
        public override TxnType TxnType => TxnType.Refund;

        /// <summary>The refund amount (in cents) for the transaction.</summary>
        [JsonPropertyName("AmtPurchase")]
        public int Amount { get; set; }

        public override ValidationResult Validate() => new RefundRequestValidator().Validate(this);
    }

    internal class RefundRequestValidator : AbstractValidator<RefundRequest>
    {
        public RefundRequestValidator()
        {
            Include(new TransactionRequestValidator());

            RuleFor(req => req.Amount).ExclusiveBetween(0, Constants.Validation.MaxAmount);
        }
    }
}