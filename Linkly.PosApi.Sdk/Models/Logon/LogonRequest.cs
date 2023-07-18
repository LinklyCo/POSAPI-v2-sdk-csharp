using FluentValidation;
using FluentValidation.Results;

namespace Linkly.PosApi.Sdk.Models.Logon
{
    /// <summary>PIN pad logon.</summary>
    public class LogonRequest : PosApiRequest
    {
        /// <summary>Specify type of logon. Defaults to <see cref="Models.LogonType.Standard" />.</summary>
        public LogonType LogonType { get; set; } = LogonType.Standard;

        public override ValidationResult Validate() => new LogonRequestValidator().Validate(this);
    }

    public class LogonRequestValidator : AbstractValidator<LogonRequest>
    {
        public LogonRequestValidator()
        {
            Include(new PosApiRequestValidator());

            RuleFor(req => req.LogonType).IsInEnum();
        }
    }
}