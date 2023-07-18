using System.Text.Json.Serialization;
using Linkly.PosApi.Sdk.Common;
using Linkly.PosApi.Sdk.Service;

namespace Linkly.PosApi.Sdk.Models
{
    /// <summary>Type of transaction to be performed.</summary>
    [JsonStringEnumMemberConverterOptions(deserializationFailureFallbackValue: Unknown)]
    public enum TxnType
    {
        /// <summary>Purchase transaction.</summary>
        [JsonPropertyName("P")] Purchase,

        /// <summary>Cash out only transaction.</summary>
        [JsonPropertyName("C")] Cash,

        /// <summary>Refund transaction.</summary>
        [JsonPropertyName("R")] Refund,

        /// <summary>Deposit transaction.</summary>
        [JsonPropertyName("D")] Deposit,

        /// <summary>Pre-authorisation.</summary>
        [JsonPropertyName("A")] PreAuth,

        /// <summary>Renew the expiry date of an existing pre-authorisation.</summary>
        [JsonPropertyName("E")] PreAuthExtend,

        /// <summary>Top-up (increase) a pre-authorisation amount.</summary>
        [JsonPropertyName("U")] PreAuthTopUp,

        /// <summary>Cancel (void) a pre-authorisation.</summary>
        [JsonPropertyName("Q")] PreAuthCancel,

        /// <summary>Reduce a pre-authorisation amount.</summary>
        [JsonPropertyName("O")] PreAuthPartialCancel,

        /// <summary>Complete a pre-authorisation.</summary>
        [JsonPropertyName("L")] PreAuthComplete,

        /// <summary>Perform an inquiry or verify the amount of a pre-authorisation.</summary>
        [JsonPropertyName("N")] PreAuthInquiry,

        /// <summary>Void transaction.</summary>
        [JsonPropertyName("I")] Void,

        /// <summary>Transaction type was not set by the PIN pad.</summary>
        [JsonPropertyName(" ")] NotSet,

        /// <summary>
        /// This value is returned when an unexpected value was deeserialised. Rather than throwing a
        /// deserialisation exception it us up to the end-user how they want to handle the error. In theory the
        /// POS should never receive a <see cref="TxnType" /> it did not initiate so this value should never be
        /// received.
        /// </summary>
        Unknown
    }

    /// <summary>Indicates the source of the customer card details.</summary>
    public enum PanSource
    {
        /// <summary>PIN pad will read card using internal card reader.</summary>
        [JsonPropertyName(" ")] PinPad,

        /// <summary>Manual Entry of card details using DateExpiry and Pan properties.</summary>
        [JsonPropertyName("K")] PosKeyed,

        /// <summary>
        /// Track2 property contains application supplied card details read directly form a magnetic
        /// stripe reader (MSR).
        /// </summary>
        [JsonPropertyName("S")] PosSwiped,

        /// <summary>Internet originated transaction.</summary>
        [JsonPropertyName("0")] Internet,

        /// <summary>Telephone originated transaction.</summary>
        [JsonPropertyName("1")] TeleOrder,

        /// <summary>Mail order originated transaction.</summary>
        [JsonPropertyName("2")] Moto,

        /// <summary>Cardholder present transaction.</summary>
        [JsonPropertyName("3")] CustomerPresent,

        /// <summary>Recurring transaction.</summary>
        [JsonPropertyName("4")] RecurringTransaction,

        /// <summary>Installment.</summary>
        [JsonPropertyName("5")] Installment
    }

    /// <summary>Type of transaction account.</summary>
    [JsonStringEnumMemberConverterOptions(deserializationFailureFallbackValue: Unknown)]
    public enum AccountType
    {
        /// <summary>Prompt customer for account type.</summary>
        [JsonPropertyName(" ")] Default,

        /// <summary>Cheque account type.</summary>
        [JsonPropertyName("1")] Cheque,

        /// <summary>Credit account type.</summary>
        [JsonPropertyName("2")] Credit,

        /// <summary>Savings account type.</summary>
        [JsonPropertyName("3")] Savings,

        /// <summary>
        /// This value is returned when an unexpected value was deeserialised. Rather than throwing a
        /// deserialisation exception it us up to the end-user how they want to handle the error.
        /// </summary>
        Unknown
    }

    /// <summary>Type of receipt.</summary>
    [JsonStringEnumMemberConverterOptions(deserializationFailureFallbackValue: Unknown)]
    public enum ReceiptType
    {
        /// <summary>Logon receipt.</summary>
        [JsonPropertyName("L")] Logon,

        /// <summary>Customer transaction receipt.</summary>
        [JsonPropertyName("C")] Customer,

        /// <summary>Merchant transaction receipt.</summary>
        [JsonPropertyName("M")] Merchant,

        /// <summary>
        /// Settlement receipt - usually contains the signature receipt line and should be printed
        /// immediately.
        /// </summary>
        [JsonPropertyName("S")] Settlement,

        /// <summary>Receipt text was received. Used internally by components and should never be received.</summary>
        [JsonPropertyName("R")] ReceiptText,

        /// <summary>
        /// This value is returned when an unexpected value was deeserialised. Rather than throwing a
        /// deserialisation exception it us up to the end-user how they want to handle the error.
        /// </summary>
        Unknown
    }

    /// <summary>Indicates how receipts from the PIN pad will be handled.</summary>
    public enum ReceiptAutoPrint
    {
        /// <summary>Return all receipts to the POS in a receipt event.</summary>
        [JsonPropertyName("0")] POS,

        /// <summary>Print all receipts from the PIN pad printer.</summary>
        [JsonPropertyName("9")] PinPad,

        /// <summary>
        /// Print all merchant/signature receipts from the PIN pad printer, return all other receipts to the
        /// POS in the transaction/logon/settlement response
        /// </summary>
        [JsonPropertyName("7")] Both
    }

    /// <summary>Indicates the EFT terminal hardware type.</summary>
    [JsonStringEnumMemberConverterOptions(deserializationFailureFallbackValue: Unknown)]
    public enum EftTerminalType
    {
        /// <summary>Ingenico NPT 710 PIN pad terminal.</summary>
        [JsonPropertyName("IngenicoNPT710")] IngenicoNPT710,

        /// <summary>Ingenico NPT PX328 PIN pad terminal.</summary>
        [JsonPropertyName("IngenicoPX328")] IngenicoPX328,

        /// <summary>Ingenico NPT i5110 PIN pad terminal.</summary>
        [JsonPropertyName("Ingenicoi5110")] Ingenicoi5110,

        /// <summary>Ingenico NPT i3070 PIN pad terminal.</summary>
        [JsonPropertyName("Ingenicoi3070")] Ingenicoi3070,

        /// <summary>Sagem PIN pad terminal.</summary>
        [JsonPropertyName("Sagem")] Sagem,

        /// <summary>Verifone PIN pad terminal.</summary>
        [JsonPropertyName("Verifone")] Verifone,

        /// <summary>Keycorp PIN pad terminal.</summary>
        [JsonPropertyName("Keycorp")] Keycorp,

        /// <summary>Linkly's Virtual PIN pad..</summary>
        [JsonPropertyName("PCEFTPOSVirtualPinpad")]
        LinklyVirtualPinPad,

        /// <summary>Unknown PIN pad terminal.</summary>
        [JsonPropertyName("Unknown")] Unknown
    }

    /// <summary>PIN pad terminal key handling scheme.</summary>
    [JsonStringEnumMemberConverterOptions(deserializationFailureFallbackValue: Unknown)]
    public enum KeyHandlingType
    {
        /// <summary>Single-DES encryption standard.</summary>
        [JsonPropertyName("1")] SingleDes,

        /// <summary>Triple-DES encryption standard.</summary>
        [JsonPropertyName("2")] TripleDes,

        /// <summary>Unknown encryption standard.</summary>
        [JsonPropertyName(" ")] Unknown
    }

    /// <summary>Type of PIN pad network connection to bank.</summary>
    [JsonStringEnumMemberConverterOptions(deserializationFailureFallbackValue: Unknown)]
    public enum NetworkType
    {
        /// <summary>Leased line bank connection.</summary>
        [JsonPropertyName("1")] Leased,

        /// <summary>Dial-up bank connection.</summary>
        [JsonPropertyName("2")] Dialup,

        /// <summary>Unknown bank connection.</summary>
        [JsonPropertyName(" ")] Unknown
    }

    /// <summary>Indicates the requested status type.</summary>
    public enum StatusType
    {
        /// <summary>Request the EFT status from the PIN pad.</summary>
        [JsonPropertyName("0")] Standard,

        /// <summary>Not supported by all PIN pads.</summary>
        [JsonPropertyName("1")] TerminalAppInfo,

        /// <summary>Not supported by all PIN pads.</summary>
        [JsonPropertyName("2")] AppCpat,

        /// <summary>Not supported by all PIN pads.</summary>
        [JsonPropertyName("3")] AppNameTable,

        [JsonPropertyName("4")] Undefined,

        /// <summary>Not supported by all PIN pads.</summary>
        [JsonPropertyName("5")] Preswipe
    }

    /// <summary>PIN pad terminal communication option</summary>
    [JsonStringEnumMemberConverterOptions(deserializationFailureFallbackValue: Unknown)]
    public enum TerminalCommsType
    {
        /// <summary>Cable link communications.</summary>
        [JsonPropertyName("0")] Cable,

        /// <summary>Intrared link communications.</summary>
        [JsonPropertyName("1")] Infrared,

        /// <summary>Unknown link communications.</summary>
        [JsonPropertyName(" ")] Unknown
    }

    /// <summary>EFTPOS settlement types.</summary>
    public enum SettlementType
    {
        /// <summary>Perform a settlement on the terminal.</summary>
        /// <remarks>Can only be performed once per day.</remarks>
        [JsonPropertyName("S")] Settlement,

        /// <summary>Perform a pre-settlement on the terminal.</summary>
        /// <remarks>Not supported by all PIN pads.</remarks>
        [JsonPropertyName("P")] PreSettlement,

        /// <summary>Perform a last settlement on the terminal .</summary>
        /// <remarks>Not supported by all PIN pads.</remarks>
        [JsonPropertyName("L")] LastSettlement,

        /// <summary>Perform a summary totals on the terminal.</summary>
        /// <remarks>Not supported by all PIN pads.</remarks>
        [JsonPropertyName("U")] SummaryTotals,

        /// <summary>Perform a shift/sub totals on the terminal.</summary>
        /// <remarks>Not supported by all PIN pads.</remarks>
        [JsonPropertyName("H")] SubShiftTotals,

        /// <summary>Perform a transaction listing on the terminal.</summary>
        /// <remarks>Not supported by all PIN pads.</remarks>
        [JsonPropertyName("I")] DetailedTransactionListing,

        /// <summary>Start cash</summary>
        /// <remarks>Not supported by all PIN pads.</remarks>
        [JsonPropertyName("M")] StartCash,

        /// <summary>SAF report</summary>
        /// <remarks>Not supported by all PIN pads.</remarks>
        [JsonPropertyName("F")] StoreAndForwardTotals,

        /// <summary>Daily cash</summary>
        /// <remarks>StGeorge agency only.</remarks>
        [JsonPropertyName("D")] DailyCashStatement
    }

    /// <summary>Indicates the type of logon to perform.</summary>
    public enum QueryCardType
    {
        /// <summary>Read card only</summary>
        [JsonPropertyName("0")] ReadCard,

        /// <summary>Read card + select account</summary>
        [JsonPropertyName("1")] ReadCardAndSelectAccount,

        /// <summary>Select account only</summary>
        [JsonPropertyName("5")] SelectAccount,

        /// <summary>Pre-swipe</summary>
        [JsonPropertyName("7")] PreSwipe,

        /// <summary>Pre-swipe special</summary>
        [JsonPropertyName("8")] PreSwipeSpecial
    }

    /// <summary>Type of logon to perform.</summary>
    public enum LogonType
    {
        /// <summary>Standard EFT logon to the bank.</summary>
        [JsonPropertyName(" ")] Standard,

        /// <summary>Standard EFT logon to the bank.</summary>
        /// <remarks>Not supported by all PIN pads.</remarks>
        [JsonPropertyName("4")] RSA,

        /// <summary>Standard EFT logon to the bank.</summary>
        /// <remarks>Not supported by all PIN pads.</remarks>
        [JsonPropertyName("5")] TmsFull,

        /// <summary>Standard EFT logon to the bank.</summary>
        /// <remarks>Not supported by all PIN pads.</remarks>
        [JsonPropertyName("6")] TmsParams,

        /// <summary>Standard EFT logon to the bank.</summary>
        /// <remarks>Not supported by all PIN pads.</remarks>
        [JsonPropertyName("7")] TmsSoftware,

        [JsonPropertyName("8")] Logoff,

        /// <summary>Enables diagnostics.</summary>
        [JsonPropertyName("1")] Diagnostics
    }

    /// <summary>Receipt re-print mode.</summary>
    public enum ReprintType
    {
        /// <summary>Get the last receipt.</summary>
        [JsonPropertyName("2")] GetLast,

        /// <summary>Re-print the last receipt.</summary>
        [JsonPropertyName("1")] Reprint
    }

    /// <summary>The card entry type of the transaction.</summary>
    [JsonStringEnumMemberConverterOptions(deserializationFailureFallbackValue: Unknown)]
    public enum CardEntryType
    {
        /// <summary>Manual entry type was not set by the PIN pad.</summary>
        [JsonPropertyName(" ")] NotSet,

        /// <summary>Unknown manual entry type. PIN pad may not support this flag.</summary>
        [JsonPropertyName("0")] Unknown,

        /// <summary>Card was swiped.</summary>
        [JsonPropertyName("S")] Swiped,

        /// <summary>Card number was keyed.</summary>
        [JsonPropertyName("K")] Keyed,

        /// <summary>Card number was read by a bar code scanner.</summary>
        [JsonPropertyName("B")] BarCode,

        /// <summary>Card number was read from a chip card.</summary>
        [JsonPropertyName("E")] ChipCard,

        /// <summary>Card number was read from a contactless reader.</summary>
        [JsonPropertyName("C")] Contactless
    }

    /// <summary>The communications method used to process the transaction.</summary>
    [JsonStringEnumMemberConverterOptions(deserializationFailureFallbackValue: Unknown)]
    public enum CommsMethodType
    {
        /// <summary>Comms method type was not set by the PIN pad.</summary>
        [JsonPropertyName(" ")] NotSet,

        /// <summary>Transaction was sent to the bank using an unknown method.</summary>
        [JsonPropertyName("0")] Unknown,

        /// <summary>Transaction was sent to the bank using a P66 modem.</summary>
        [JsonPropertyName("1")] P66,

        /// <summary>Transaction was sent to the bank using an Argent.</summary>
        [JsonPropertyName("2")] Argent,

        /// <summary>Transaction was sent to the bank using an X25.</summary>
        [JsonPropertyName("3")] X25
    }

    /// <summary>The currency conversion status for the transaction.</summary>
    [JsonStringEnumMemberConverterOptions(deserializationFailureFallbackValue: Unknown)]
    public enum CurrencyStatus
    {
        /// <summary>Currency conversion status was not set by the PIN pad.</summary>
        [JsonPropertyName(" ")] NotSet,

        /// <summary>Transaction amount was processed in Australian Dollars.</summary>
        [JsonPropertyName("0")] AUD,

        /// <summary>Transaction amount was currency converted.</summary>
        [JsonPropertyName("1")] Converted,

        /// <summary>
        /// This value is returned when an unexpected value was deeserialised. Rather than throwing a
        /// deserialisation exception it us up to the end-user how they want to handle the error.
        /// </summary>
        Unknown
    }

    /// <summary>The Pay Pass status of the transaction.</summary>
    [JsonStringEnumMemberConverterOptions(deserializationFailureFallbackValue: Unknown)]
    public enum PayPassStatus
    {
        /// <summary>Pay Pass conversion status was not set by the PIN pad.</summary>
        [JsonPropertyName(" ")] NotSet,

        /// <summary>Pay Pass was used in the transaction.</summary>
        [JsonPropertyName("1")] PayPassUsed,

        /// <summary>Pay Pass was not used in the transaction.</summary>
        [JsonPropertyName("0")] PayPassNotUsed,

        /// <summary>
        /// This value is returned when an unexpected value was deeserialised. Rather than throwing a
        /// deserialisation exception it us up to the end-user how they want to handle the error.
        /// </summary>
        Unknown
    }

    [JsonStringEnumMemberConverterOptions(deserializationFailureFallbackValue: Unknown)]
    public enum InputType
    {
        [JsonPropertyName("0")] None,

        [JsonPropertyName("1")] Normal,

        [JsonPropertyName("2")] Amount,

        [JsonPropertyName("3")] Decimal,

        [JsonPropertyName("4")] Password,

        /// <summary>
        /// This value is returned when an unexpected value was deeserialised. Rather than throwing a
        /// deserialisation exception it us up to the end-user how they want to handle the error.
        /// </summary>
        Unknown
    }

    [JsonStringEnumMemberConverterOptions(deserializationFailureFallbackValue: Unknown)]
    public enum GraphicCode
    {
        [JsonPropertyName("0")] Processing,

        [JsonPropertyName("1")] Verify,

        [JsonPropertyName("2")] Question,

        [JsonPropertyName("3")] Card,

        [JsonPropertyName("4")] Account,

        [JsonPropertyName("5")] PIN,

        [JsonPropertyName("6")] Finished,

        [JsonPropertyName(" ")] None,

        /// <summary>
        /// This value is returned when an unexpected value was deeserialised. Rather than throwing a
        /// deserialisation exception it us up to the end-user how they want to handle the error.
        /// </summary>
        Unknown
    }

    /// <summary>Type of API response.</summary>
    [JsonStringEnumMemberConverterOptions(deserializationFailureFallbackValue: Unknown)]
    public enum ResponseType
    {
        /// <summary>Response is for a <see cref="Models.Logon.LogonRequest" />.</summary>
        [JsonPropertyName(Constants.ResponseType.Logon)]
        Logon,

        /// <summary>Response is for a <see cref="Models.Status.StatusRequest" />.</summary>
        [JsonPropertyName(Constants.ResponseType.Status)]
        Status,

        /// <summary>Response contains text the PIN pad is displaying.</summary>
        [JsonPropertyName(Constants.ResponseType.Display)]
        Display,

        /// <summary>Response contains a receipt.</summary>
        [JsonPropertyName(Constants.ResponseType.Receipt)]
        Receipt,

        /// <summary>Response is for a <see cref="Models.ConfigureMerchant.ConfigureMerchantRequest" /> request.</summary>
        [JsonPropertyName(Constants.ResponseType.ConfigureMerchant)]
        ConfigureMerchant,

        /// <summary>Response is for a <see cref="Models.QueryCard.QueryCardRequest" /> request.</summary>
        [JsonPropertyName(Constants.ResponseType.QueryCard)]
        QueryCard,

        /// <summary>Response is for a <see cref="Models.ReprintReceipt.ReprintReceiptRequest" /> request.</summary>
        [JsonPropertyName(Constants.ResponseType.ReprintReceipt)]
        ReprintReceipt,

        /// <summary>Response is for a <see cref="Models.Transaction.TransactionRequest" /> request.</summary>
        [JsonPropertyName(Constants.ResponseType.Transaction)]
        Transaction,

        /// <summary>Response is for a <see cref="Models.Settlement.SettlementRequest" /> request.</summary>
        [JsonPropertyName(Constants.ResponseType.Settlement)]
        Settlement,

        /// <summary>
        /// This value is returned when an unexpected value was deeserialised. Rather than throwing a
        /// deserialisation exception it us up to the end-user how they want to handle the error.
        /// </summary>
        Unknown
    }

    /// <summary>Source causing the <see cref="IPosApiEventListener.Error" /> handler to be invoked.</summary>
    public enum ErrorSource
    {
        /// <summary>
        /// Used to indicate <see cref="PosApiService" /> was able to receive a response from the API however
        /// the HTTP response returned an error code. This could be caused by authentication failure, an
        /// invalid request URI, erroneous data within the request, etc.
        /// </summary>
        API,

        /// <summary>
        /// This code indicates an exception was caught by <see cref="PosApiService" />. This exception should
        /// be less common than <see cref="API" /> and would most likely be caused by connectivity issues with
        /// the API.
        /// </summary>
        Internal
    }

    /// <summary>
    /// Used by <see cref="Models.Transaction.RetrieveTransactionRequest" /> to control which identifier should be
    /// used to search for historical transactions.
    /// </summary>
    public enum ReferenceType
    {
        /// <summary>Use <see cref="Models.Transaction.TransactionRequest.TxnRef" />.</summary>
        ReferenceNo,

        /// <summary>Use <see cref="Models.Transaction.TransactionRequest.RRN" />.</summary>
        RRN
    }
}