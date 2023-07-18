using System;
using System.Collections.ObjectModel;
using System.Text.Json.Serialization;
using Linkly.PosApi.Sdk.Common;
using Linkly.PosApi.Sdk.Models.Receipt;

namespace Linkly.PosApi.Sdk.Models.Transaction
{
    /// <summary>Response to all types of transaction requests.</summary>
    public class TransactionResponse : PosApiResponseWithResult
    {
        /// <summary>The type of transaction to perform.</summary>
        /// <example>P</example>
        public TxnType TxnType { get; set; }

        /// <summary>Two digit merchant code.</summary>
        /// <example>00</example>
        public string Merchant { get; set; } = null!;

        /// <summary>
        /// Indicates the card type used for the transaction as described by the bank. Examples may include:
        /// 'AMEX', 'VISA', 'DEBIT'. The possible values of this field may change between acquirers and PIN pad
        /// versions. To identify the payment type used, reference the <see cref="CardName" /> field.
        /// </summary>
        /// <example>AMEX CARD           </example>
        public string CardType { get; set; } = null!;

        /// <summary>BIN number of the card. For a complete list refer to the API documentation.</summary>
        /// <remarks>
        /// <list type="table">
        /// <listheader><term>Card BIN</term><description>Card Type</description></listheader>
        /// <item><term>0</term><description>Unknown</description></item>
        /// <item><term>1</term><description>Debit</description></item>
        /// <item><term>2</term><description>Bankcard</description></item>
        /// <item><term>3</term><description>Mastercard</description></item>
        /// <item><term>4</term><description>Visa</description></item>
        /// <item><term>5</term><description>American Express</description></item>
        /// <item><term>6</term><description>Diner Club</description></item>
        /// <item><term>7</term><description>JCB</description></item>
        /// <item><term>8</term><description>Label Card</description></item>
        /// <item><term>9</term><description>JCB</description></item>
        /// <item><term>11</term><description>JCB</description></item>
        /// <item><term>12</term><description>Other</description></item>
        /// </list>
        /// </remarks>
        public string CardName { get; set; } = null!;

        /// <summary>The retrieval reference number for the transaction.</summary>
        /// <example>123456789012</example>
        public string? RRN { get; set; }

        /// <summary>Indicates which settlement batch this transaction will be included in.</summary>
        public DateTime DateSettlement { get; set; }

        /// <summary>The cash amount (in cents) for the transaction.</summary>
        /// <example>0</example>
        [JsonPropertyName("AmtCash")]
        public int AmountCash { get; set; }

        /// <summary>The purchase amount (in cents) for the transaction.</summary>
        /// <example>2345</example>
        [JsonPropertyName("AmtPurchase")]
        public int Amount { get; set; }

        /// <summary>The tip amount (in cents) for the transaction.</summary>
        /// <example>0</example>
        [JsonPropertyName("AmtTip")]
        public int AmountTip { get; set; }

        /// <summary>The authorisation number for the transaction.</summary>
        /// <example>0</example>
        public int AuthCode { get; set; }

        /// <summary>The reference number to attach to the transaction. This will appear on the receipt.</summary>
        /// <example>1234567890</example>
        public string TxnRef { get; set; } = null!;

        /// <summary>The card number to use when pan source of POS keyed is used.</summary>
        /// <example>37601234567890      </example>
        public string PAN { get; set; } = null!;

        /// <summary>The expiry date of the card when of POS keyed is used. In MMYY format.</summary>
        public string DateExpiry { get; set; } = null!;

        /// <summary>The track 2 to use when of POS swiped is used.</summary>
        /// <example>37601234567890=0949                     </example>
        public string Track2 { get; set; } = null!;

        /// <summary>The account to use for this transaction. Use ' ' to prompt user to enter the account type</summary>
        /// <example>2</example>
        public AccountType AccountType { get; set; }

        /// <summary>Flags that indicate how the transaction was processed.</summary>
        public TransactionFlags TxnFlags { get; set; } = new TransactionFlags();

        /// <summary>Indicates if an available balance is present in the response.</summary>
        public bool? BalanceReceived { get; set; }

        /// <summary>Balance available on the processed account.</summary>
        public int? AvailableBalance { get; set; }

        /// <summary>Cleared balance on the processed account.</summary>
        public int? ClearedFundsBalance { get; set; }

        /// <summary>Date and time of the response returned by the bank.</summary>
        public DateTime Date { get; set; }

        /// <summary>Terminal ID configured in the PIN pad.</summary>
        /// <example>12345678</example>
        public string CatId { get; set; } = null!;

        /// <summary>Merchant ID configured in the PIN pad.</summary>
        /// <example>0123456789ABCDEF</example>
        public string CaId { get; set; } = null!;

        /// <summary>System Trace Audit Number</summary>
        public int Stan { get; set; }

        /// <summary>
        /// Transaction reference number returned by the bank or acquirer, required for follow-up transactions
        /// such as refunds, pre-auth completions, pre-auth cancellations, etc. This value comes from
        /// <see cref="PosApiResponse.PurchaseAnalysisData" />.
        /// </summary>
        public string? RFN => PurchaseAnalysisData.GetValueOrDefault(Constants.PurchaseAnalysisData.RFN);

        /// <summary>
        /// If tipping or surcharging is enabled this should return the total transaction amount (in cents)
        /// including tips and surcharges. This value comes from
        /// <see cref="PosApiResponse.PurchaseAnalysisData" />.
        /// </summary>
        public int? AmountTotal =>
            int.TryParse(PurchaseAnalysisData.GetValueOrDefault(Constants.PurchaseAnalysisData.Amount), out var total)
                ? total
                : (int?)null;

        /// <summary>
        /// If surcharging is enabled this should return the surcharge amount (in cents). This value comes from
        /// <see cref="PosApiResponse.PurchaseAnalysisData" />.
        /// </summary>
        public int? AmountSurcharge => int.TryParse(PurchaseAnalysisData.GetValueOrDefault(Constants.PurchaseAnalysisData.Surcharge),
            out var surcharge)
            ? surcharge
            : (int?)null;

        /// <summary>Receipts generated by the transaction.</summary>
        public Collection<ReceiptResponse> Receipts { get; set; } = new Collection<ReceiptResponse>();
    }
}