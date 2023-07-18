using FluentValidation;
using FluentValidation.Results;

namespace Linkly.PosApi.Sdk.Models.ReprintReceipt
{
    /// <summary>Re-print receipt.</summary>
    public class ReprintReceiptRequest : PosApiRequest
    {
        /// <summary>Indicates whether the receipt should be returned or reprinted.</summary>
        public ReprintType ReprintType { get; set; } = ReprintType.GetLast;

        public override ValidationResult Validate() => new ReprintReceiptRequestValidator().Validate(this);
    }

    internal class ReprintReceiptRequestValidator : AbstractValidator<ReprintReceiptRequest>
    {
        public ReprintReceiptRequestValidator()
        {
            Include(new PosApiRequestValidator());

            RuleFor(req => req.ReprintType).IsInEnum();
        }
    }
}