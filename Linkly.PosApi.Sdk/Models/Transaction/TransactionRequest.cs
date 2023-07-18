using System.Diagnostics.CodeAnalysis;
using FluentValidation;
using Linkly.PosApi.Sdk.Common;

namespace Linkly.PosApi.Sdk.Models.Transaction
{
    /// <summary>Base request used for all transactions.</summary>
    public abstract class TransactionRequest : PosApiRequest
    {
        /// <summary>
        /// Type of transaction to perform. This field is set by the specific request class and cannot
        /// be changed.
        /// </summary>
        public abstract TxnType TxnType { get; }

        /// <summary>The currency code for this transaction (e.g. AUD). A 3 digit ISO currency code.</summary>
        public string? CurrencyCode { get; set; }

        /// <summary>
        /// Indicates if the transaction supports tipping.. Set to TRUE if tipping is to be enabled
        /// for this transaction.
        /// </summary>
        public bool EnableTip { get; set; }

        /// <summary>
        /// Determines if the transaction is a training mode transaction. Set to TRUE if the transaction is to
        /// be performed in training mode. The default is false.
        /// </summary>
        public bool TrainingMode { get; set; }

        /// <summary>The reference number to attach to the transaction. This will appear on the receipt.</summary>
        public string TxnRef { get; set; } = null!;

        /// <summary>The authorisation number for the transaction.</summary>
        public int? AuthCode { get; set; }

        /// <summary>
        /// Indicates the source of the customer card details. The default is
        /// <see cref="Models.PanSource.PinPad" />.
        /// </summary>
        public PanSource PanSource { get; set; } = PanSource.PinPad;

        /// <summary>
        /// The card number to use when pan source of POS keyed is used. Use this property in conjunction with
        /// <see cref="PanSource" />.
        /// </summary>
        public string? PAN { get; set; }

        /// <summary>
        /// The expiry date of the card when of POS keyed is used. In MMYY format. Use this property in
        /// conjunction with <see cref="PanSource" /> when passing the card expiry date to Linkly.
        /// </summary>
        public string? DateExpiry { get; set; }

        /// <summary>
        /// The track 2 to use when of POS swiped is used. Use this property in conjunction with
        /// <see cref="PanSource" />.
        /// </summary>
        public string? Track2 { get; set; }

        /// <summary>
        /// The account to use for this transaction. Defaults to <see cref="Models.AccountType.Default" />
        /// </summary>
        public AccountType AccountType { get; set; } = AccountType.Default;

        /// <summary>
        /// The retrieval reference number for the transaction. Only required for some transaction
        /// types
        /// </summary>
        public string? RRN { get; set; }

        /// <summary>Card verification value.</summary>
        public int? CVV { get; set; }

        /// <summary>
        /// Set tip options to present to the cardholder. The options are different tip percentages for the
        /// terminal to present. When set, the terminal is to display each of the options along with providing
        /// the ability for a cardholder to enter their own dollar amount. This setter updates
        /// <see cref="PosApiRequest.PurchaseAnalysisData" />.
        /// </summary>
        public void SetTipOptions(TippingOptions tippingOptions)
        {
            PurchaseAnalysisData[Constants.PurchaseAnalysisData.TPO] = ((IPurchaseAnalysisDataConverter)tippingOptions).ToPadString();
        }

        /// <summary>
        /// Set the tip amount (in cents) to apply on top of the transaction amount. The tip will be
        /// included on the receipt. This setter updates <see cref="PosApiRequest.PurchaseAnalysisData" />.
        /// </summary>
        [SuppressMessage("SonarLint", "S2376", Justification = "Simplifies initialisation of the model")]
        public uint TipAmount
        {
            set => PurchaseAnalysisData[Constants.PurchaseAnalysisData.Tip] = value.ToString();
        }

        /// <summary>
        /// Set the product level blocking flag, indicating whether restricted item(s) are present in the sale.
        /// This flag is optional unless the merchant is participating in the DSS PLB program, in which case it
        /// is mandatory. Setting this property to true indicates restricted item(s) are present in the sale
        /// and product level blocking is required. If false no restricted items are present in the sale and
        /// product level blocking is not required. This setter updates
        /// <see cref="PosApiRequest.PurchaseAnalysisData" />.
        /// </summary>
        [SuppressMessage("SonarLint", "S2376", Justification = "Simplifies initialisation of the model")]
        public bool ProductLevelBlock
        {
            set => PurchaseAnalysisData[Constants.PurchaseAnalysisData.PLB] = value ? "1" : "0";
        }

        /// <summary>
        /// Set the surcharges rules to apply based on the card bin. This setter updates
        /// <see cref="PosApiRequest.PurchaseAnalysisData" />.
        /// </summary>
        public void SetSurchargeOptions(SurchargeOptions surchargeOptions)
        {
            PurchaseAnalysisData[Constants.PurchaseAnalysisData.SC2] = ((IPurchaseAnalysisDataConverter)surchargeOptions).ToPadString();
        }
    }

    internal class TransactionRequestValidator : AbstractValidator<TransactionRequest>
    {
        public TransactionRequestValidator()
        {
            Include(new PosApiRequestValidator());

            RuleFor(req => req.CurrencyCode).Length(3);
            RuleFor(req => req.TxnRef).NotEmpty().MaximumLength(32);
            RuleFor(req => req.AuthCode).InclusiveBetween(0, 999999);
            RuleFor(req => req.PanSource).IsInEnum();
            RuleFor(req => req.PAN).Length(20);
            RuleFor(req => req.DateExpiry).CardExpiryDate();
            RuleFor(req => req.Track2).Length(40);
            RuleFor(req => req.AccountType).IsInEnum();
            RuleFor(req => req.RRN).Length(12);

            When(req => req.PanSource == PanSource.PosKeyed, () =>
            {
                RuleFor(req => req.PAN).NotEmpty();
                RuleFor(req => req.DateExpiry).NotEmpty();
            });

            When(req => req.PanSource == PanSource.PosSwiped, () => { RuleFor(req => req.Track2).NotEmpty(); });
        }
    }
}