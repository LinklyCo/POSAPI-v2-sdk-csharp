using System.Text.Json.Serialization;
using FluentValidation;
using Linkly.PosApi.Sdk.Common;

namespace Linkly.PosApi.Sdk.Models.Transaction
{
    /// <summary>
    /// Abstract base class for pre-authorisation requests which perform some action on an
    /// existing pre-authorisation.
    /// </summary>
    public abstract class PreAuthManipulationRequest : FollowUpTransactionRequest
    {
        [JsonPropertyName("AmtPurchase")] public int Amount { get; set; }
    }

    public class PreAuthManipulationRequestValidator : AbstractValidator<PreAuthManipulationRequest>
    {
        public PreAuthManipulationRequestValidator()
        {
            Include(new FollowUpTransactionRequestValidator());

            RuleFor(req => req.Amount).InclusiveBetween(1, Constants.Validation.MaxAmount);
        }
    }
}