using FluentValidation;
using FluentValidation.Results;

namespace Linkly.PosApi.Sdk.Models.QueryCard
{
    /// <summary>Query the details of a card.</summary>
    public class QueryCardRequest : PosApiRequest
    {
        /// <summary>Type of querycard to perform.</summary>
        public QueryCardType QueryCardType { get; set; } = QueryCardType.ReadCard;

        public override ValidationResult Validate() => new QueryCardRequestValidator().Validate(this);
    }

    public class QueryCardRequestValidator : AbstractValidator<QueryCardRequest>
    {
        public QueryCardRequestValidator()
        {
            Include(new PosApiRequestValidator());

            RuleFor(req => req.QueryCardType).IsInEnum();
        }
    }
}