using System.Diagnostics.CodeAnalysis;
using FluentValidation;
using Linkly.PosApi.Sdk.Common;

namespace Linkly.PosApi.Sdk.Models.Transaction
{
    /// <summary>Transaction request which is linked to an original transaction via a reference number.</summary>
    public abstract class FollowUpTransactionRequest : TransactionRequest
    {
        /// <summary>
        /// Sets the reference number of the original transaction on a subsequent transaction.
        /// This setter updates <see cref="PosApiRequest.PurchaseAnalysisData" />.
        /// </summary>
        [SuppressMessage("SonarLint", "S2376", Justification = "Simplifies initialisation of the model")]
        public string RFN
        {
            set => PurchaseAnalysisData[Constants.PurchaseAnalysisData.RFN] = value;
        }
    }

    public class FollowUpTransactionRequestValidator : AbstractValidator<FollowUpTransactionRequest>
    {
        public FollowUpTransactionRequestValidator()
        {
            Include(new TransactionRequestValidator());

            RuleFor(req => req.PurchaseAnalysisData).HasValue(Constants.PurchaseAnalysisData.RFN);
        }
    }
}