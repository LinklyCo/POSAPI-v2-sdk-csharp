using FluentValidation.Results;

namespace Linkly.PosApi.Sdk.Models.Transaction
{
    /// <summary>Renew the expiry date of a <see cref="PreAuthRequest" />.</summary>
    public class PreAuthExtendRequest : FollowUpTransactionRequest
    {
        public override TxnType TxnType => TxnType.PreAuthExtend;

        public override ValidationResult Validate() => new FollowUpTransactionRequestValidator().Validate(this);
    }
}