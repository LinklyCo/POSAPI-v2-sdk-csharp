using FluentValidation;
using FluentValidation.Results;

namespace Linkly.PosApi.Sdk.Models.Settlement
{
    /// <summary>Get the last settlement details.</summary>
    public class SettlementRequest : PosApiRequest
    {
        /// <summary>EFT settlement type. Defaults to <see cref="Models.SettlementType.Settlement" /></summary>
        public SettlementType SettlementType { get; set; } = SettlementType.Settlement;

        /// <summary>
        /// Reset totals after settlement. Only used for settlement type
        /// <see cref="Models.SettlementType.SubShiftTotals" />.
        /// </summary>
        public bool ResetTotals { get; set; }

        public override ValidationResult Validate() => new SettlementRequestValidator().Validate(this);
    }

    internal class SettlementRequestValidator : AbstractValidator<SettlementRequest>
    {
        public SettlementRequestValidator()
        {
            Include(new PosApiRequestValidator());

            RuleFor(req => req.SettlementType).IsInEnum();
        }
    }
}