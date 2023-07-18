using FluentValidation.Results;

namespace Linkly.PosApi.Sdk.Models.Transaction
{
    /// <summary>
    /// Complete a <see cref="PreAuthRequest" /> for an amount of
    /// <see cref="PreAuthManipulationRequest.Amount" />.
    /// </summary>
    public class PreAuthCompletionRequest : PreAuthManipulationRequest
    {
        public override TxnType TxnType => TxnType.PreAuthComplete;

        public override ValidationResult Validate() => new PreAuthManipulationRequestValidator().Validate(this);
    }
}