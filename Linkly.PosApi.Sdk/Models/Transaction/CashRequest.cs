using System.Text.Json.Serialization;
using FluentValidation;
using FluentValidation.Results;
using Linkly.PosApi.Sdk.Common;

namespace Linkly.PosApi.Sdk.Models.Transaction
{
    /// <summary>Cash-out only transaction.</summary>
    public class CashRequest : TransactionRequest
    {
        public override TxnType TxnType => TxnType.Cash;

        /// <summary>The cash out amount.</summary>
        [JsonPropertyName("AmtCash")]
        public int AmountCash { get; set; }

        public override ValidationResult Validate() => new CashRequestValidator().Validate(this);
    }

    internal class CashRequestValidator : AbstractValidator<CashRequest>
    {
        public CashRequestValidator()
        {
            Include(new TransactionRequestValidator());

            RuleFor(req => req.AmountCash).InclusiveBetween(1, Constants.Validation.MaxAmount);
        }
    }
}