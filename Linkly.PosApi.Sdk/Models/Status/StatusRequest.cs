using FluentValidation;
using FluentValidation.Results;

namespace Linkly.PosApi.Sdk.Models.Status
{
    /// <summary>Get the terminal status.</summary>
    public class StatusRequest : PosApiRequest
    {
        /// <summary>Type of status to perform. Defaults to <see cref="Models.StatusType.Standard" /></summary>
        public StatusType StatusType { get; set; } = StatusType.Standard;

        public override ValidationResult Validate() => new StatusRequestValidator().Validate(this);
    }

    internal class StatusRequestValidator : AbstractValidator<StatusRequest>
    {
        public StatusRequestValidator()
        {
            Include(new PosApiRequestValidator());

            RuleFor(req => req.StatusType).IsInEnum();
        }
    }
}