using FluentValidation.Results;

namespace Linkly.PosApi.Sdk.Models.Transaction
{
    /// <summary>
    /// Increase a <see cref="PreAuthRequest" /> for an amount of
    /// <see cref="PreAuthManipulationRequest.Amount" /> cents.
    /// </summary>
    public class PreAuthTopUpRequest : PreAuthManipulationRequest
    {
        public override TxnType TxnType => TxnType.PreAuthTopUp;

        public override ValidationResult Validate() => new PreAuthManipulationRequestValidator().Validate(this);
    }
}