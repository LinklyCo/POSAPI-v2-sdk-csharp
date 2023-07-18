using FluentValidation;
using FluentValidation.Results;

namespace Linkly.PosApi.Sdk.Models.Authentication
{
    /// <summary>Pair a PIN pad to the POS.</summary>
    public class PairingRequest : IBaseRequest, IValidatable
    {
        /// <summary>Linkly Cloud username.</summary>
        public string Username { get; set; } = null!;

        /// <summary>Linkly Cloud password.</summary>
        public string Password { get; set; } = null!;

        /// <summary>The pair-code as displayed on the PIN pad.</summary>
        public string PairCode { get; set; } = null!;

        /// <summary>Validate the model using the fluent validator.</summary>
        /// <returns>Validation result containing model errors (if any.)</returns>
        public ValidationResult Validate() => new PairingRequestValidator().Validate(this);
    }

    internal class PairingRequestValidator : AbstractValidator<PairingRequest>
    {
        public PairingRequestValidator()
        {
            RuleFor(req => req.Username).NotEmpty();
            RuleFor(req => req.Password).NotEmpty();
            RuleFor(req => req.PairCode).NotEmpty();
        }
    }
}