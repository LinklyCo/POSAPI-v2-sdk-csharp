using System.Text.Json.Serialization;
using FluentValidation;
using FluentValidation.Results;
using Linkly.PosApi.Sdk.Common;

namespace Linkly.PosApi.Sdk.Models.Transaction
{
    /// <summary>Deposit cash and/or cheques.</summary>
    public class DepositRequest : TransactionRequest
    {
        public override TxnType TxnType => TxnType.Deposit;

        /// <summary>The cash deposit amount.</summary>
        [JsonPropertyName("AmtCash")]
        public int AmountCash { get; set; }

        /// <summary>The chequedeposit amount.</summary>
        [JsonPropertyName("AmtPurchase")]
        public int AmountCheque { get; set; }

        /// <summary>Total number of cheques to deposit.</summary>
        [JsonPropertyName("TotalPurchaseCount")]
        public int TotalCheques { get; set; }

        public override ValidationResult Validate() => new DepositRequestValidator().Validate(this);
    }

    internal class DepositRequestValidator : AbstractValidator<DepositRequest>
    {
        public DepositRequestValidator()
        {
            Include(new TransactionRequestValidator());

            RuleFor(req => req.AmountCash)
                .InclusiveBetween(0, Constants.Validation.MaxAmount)
                .GreaterThan(1).When(req => req.AmountCheque == 0, ApplyConditionTo.CurrentValidator);
            RuleFor(req => req.AmountCheque)
                .InclusiveBetween(0, Constants.Validation.MaxAmount)
                .GreaterThan(1).When(req => req.AmountCash == 0, ApplyConditionTo.CurrentValidator);

            RuleFor(req => req.TotalCheques).GreaterThanOrEqualTo(0);
        }
    }
}