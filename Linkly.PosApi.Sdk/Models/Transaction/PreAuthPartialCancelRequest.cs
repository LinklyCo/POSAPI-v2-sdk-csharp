using FluentValidation.Results;

namespace Linkly.PosApi.Sdk.Models.Transaction
{
    /// <summary>
    /// Reduce a <see cref="PreAuthRequest" /> for an amount of
    /// <see cref="PreAuthManipulationRequest.Amount" /> cents.
    /// </summary>
    public class PreAuthPartialCancelRequest : PreAuthManipulationRequest
    {
        public override TxnType TxnType => TxnType.PreAuthPartialCancel;

        public override ValidationResult Validate() => new PreAuthManipulationRequestValidator().Validate(this);
    }
}