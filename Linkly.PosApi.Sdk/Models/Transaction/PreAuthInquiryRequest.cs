using FluentValidation.Results;

namespace Linkly.PosApi.Sdk.Models.Transaction
{
    /// <summary>Perform an inquiry or verify the amount of a <see cref="PreAuthRequest" />.</summary>
    public class PreAuthInquiryRequest : FollowUpTransactionRequest
    {
        public override TxnType TxnType => TxnType.PreAuthInquiry;

        public override ValidationResult Validate() => new FollowUpTransactionRequestValidator().Validate(this);
    }
}