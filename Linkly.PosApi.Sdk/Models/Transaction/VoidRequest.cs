using FluentValidation;
using FluentValidation.Results;
using Linkly.PosApi.Sdk.Common;
using System.Text.Json.Serialization;

namespace Linkly.PosApi.Sdk.Models.Transaction
{
    /// <summary>Void transaction prior to settlement.</summary>
    public class VoidRequest : TransactionRequest
    {
        public override TxnType TxnType => TxnType.Void;

        [JsonPropertyName("AmtPurchase")]
        public int Amount { get; set; }

        public override ValidationResult Validate() => new VoidRequestValidator().Validate(this);
    }

    internal class VoidRequestValidator : AbstractValidator<VoidRequest>
    {
        public VoidRequestValidator()
        {
            Include(new TransactionRequestValidator());

            RuleFor(req => req.Amount).InclusiveBetween(1, Constants.Validation.MaxAmount);
        }
    }
}