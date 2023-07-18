using System;
using FluentValidation;
using FluentValidation.Results;

namespace Linkly.PosApi.Sdk.Models.Result
{
    // <summary>Send a request to the API to retrieve the responses of a session.</summary>
    public class ResultRequest : IBaseRequest, IValidatable
    {
        public ResultRequest(Guid sessionId)
        {
            SessionId = sessionId;
        }

        /// <summary>Session ID for which to retrieve the responses.</summary>
        public Guid SessionId { get; set; }

        public ValidationResult Validate() => new ResultRequestValidator().Validate(this);
    }

    public class ResultRequestValidator : AbstractValidator<ResultRequest>
    {
        public ResultRequestValidator()
        {
            RuleFor(req => req.SessionId).NotEmpty();
        }
    }
}