using FluentValidation;
using FluentValidation.Results;

namespace Linkly.PosApi.Sdk.Models.Transaction
{
    /// <summary>Retrieve a historical transaction result.</summary>
    public class RetrieveTransactionRequest : IBaseRequest, IValidatable
    {
        /// <summary>Identifier which should be used to search for historical transactions.</summary>
        public ReferenceType ReferenceType { get; set; } = ReferenceType.ReferenceNo;

        /// <summary>Reference identifier to search for.</summary>
        public string Reference { get; set; } = null!;

        public ValidationResult Validate() => new RetrieveTransactionRequestValidator().Validate(this);
    }

    internal class RetrieveTransactionRequestValidator : AbstractValidator<RetrieveTransactionRequest>
    {
        public RetrieveTransactionRequestValidator()
        {
            RuleFor(req => req.ReferenceType).IsInEnum();
            RuleFor(req => req.Reference).NotEmpty();
        }
    }
}