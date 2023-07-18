using System.Diagnostics.CodeAnalysis;
using FluentValidation.Results;
using Linkly.PosApi.Sdk.Common;

namespace Linkly.PosApi.Sdk.Models.Transaction
{
    /// <summary>Perform an inquiry on all existing pre-authorisations for informational purposes.</summary>
    public class PreAuthSummaryRequest : TransactionRequest
    {
        public override TxnType TxnType => TxnType.PreAuthInquiry;

        /// <summary>
        /// Sets the summary window number to request. If null then the first summary window will be
        /// requested. Example: <example>1</example>. This setter updates
        /// <see cref="PosApiRequest.PurchaseAnalysisData" />.
        /// </summary>
        [SuppressMessage("SonarLint", "S2376", Justification = "Simplifies initialisation of the model")]
        public uint PreAuthIndex
        {
            set => PurchaseAnalysisData[Constants.PurchaseAnalysisData.PAI] = value.ToString();
        }

        public override ValidationResult Validate() => new TransactionRequestValidator().Validate(this);
    }
}