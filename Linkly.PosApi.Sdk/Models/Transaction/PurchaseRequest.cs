using System.Text.Json.Serialization;
using FluentValidation;
using FluentValidation.Results;
using Linkly.PosApi.Sdk.Common;

namespace Linkly.PosApi.Sdk.Models.Transaction
{
    /// <summary>Purchase transaction.</summary>
    public class PurchaseRequest : TransactionRequest
    {
        public override TxnType TxnType => TxnType.Purchase;

        /// <summary>The purchase amount (in cents) of the transaction.</summary>
        [JsonPropertyName("AmtPurchase")]
        public int Amount { get; set; }

        /// <summary>The cash out amount (in cents) of the transaction. Defaults to 0</summary>
        [JsonPropertyName("AmtCash")]
        public int? AmountCash { get; set; }

        public override ValidationResult Validate() => new PurchaseRequestValidator().Validate(this);
    }

    internal class PurchaseRequestValidator : AbstractValidator<PurchaseRequest>
    {
        public PurchaseRequestValidator()
        {
            Include(new TransactionRequestValidator());

            RuleFor(req => req.Amount).InclusiveBetween(1, Constants.Validation.MaxAmount);
            RuleFor(req => req.AmountCash).InclusiveBetween(0, Constants.Validation.MaxAmount);
        }
    }
}