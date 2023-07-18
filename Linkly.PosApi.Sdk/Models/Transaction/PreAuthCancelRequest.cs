using FluentValidation.Results;

namespace Linkly.PosApi.Sdk.Models.Transaction
{
    /// <summary>Cancel a <see cref="PreAuthRequest" />.</summary>
    public class PreAuthCancelRequest : FollowUpTransactionRequest
    {
        public override TxnType TxnType => TxnType.PreAuthCancel;

        public override ValidationResult Validate() => new FollowUpTransactionRequestValidator().Validate(this);
    }
}