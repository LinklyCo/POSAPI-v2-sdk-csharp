using Linkly.PosApi.Sdk.Models;

namespace Linkly.PosApi.Sdk.UnitTest.Common;

/// <summary>Map enums in the SDK to the corresponding string values in the API.</summary>
internal static class EnumMapper
{
    public static readonly IReadOnlyDictionary<ResponseType, string> ResponseTypeMap = new Dictionary<ResponseType, string>
    {
        [ResponseType.Logon] = "logon",
        [ResponseType.Status] = "status",
        [ResponseType.Display] = "display",
        [ResponseType.Receipt] = "receipt",
        [ResponseType.ConfigureMerchant] = "configuremerchant",
        [ResponseType.QueryCard] = "querycard",
        [ResponseType.ReprintReceipt] = "reprintreceipt",
        [ResponseType.Transaction] = "transaction",
        [ResponseType.Settlement] = "settlement"
    };

    public static readonly IReadOnlyDictionary<ReceiptAutoPrint, string> ReceiptAutoPrintMap = new Dictionary<ReceiptAutoPrint, string>
    {
        [ReceiptAutoPrint.POS] = "0",
        [ReceiptAutoPrint.PinPad] = "9",
        [ReceiptAutoPrint.Both] = "7"
    };

    public static readonly IReadOnlyDictionary<TxnType, string> TxnTypeMap = new Dictionary<TxnType, string>
    {
        [TxnType.Purchase] = "P",
        [TxnType.Cash] = "C",
        [TxnType.Refund] = "R",
        [TxnType.Deposit] = "D",
        [TxnType.PreAuth] = "A",
        [TxnType.PreAuthExtend] = "E",
        [TxnType.PreAuthTopUp] = "U",
        [TxnType.PreAuthCancel] = "Q",
        [TxnType.PreAuthPartialCancel] = "O",
        [TxnType.PreAuthComplete] = "L",
        [TxnType.PreAuthInquiry] = "N",
        [TxnType.Void] = "I"
    };

    public static readonly IReadOnlyDictionary<PanSource, string> PanSourceMap = new Dictionary<PanSource, string>
    {
        [PanSource.PinPad] = " ",
        [PanSource.PosKeyed] = "K"
    };

    public static readonly IReadOnlyDictionary<AccountType, string> AccountTypeMap = new Dictionary<AccountType, string>
    {
        [AccountType.Default] = " ",
        [AccountType.Credit] = "2",
        [AccountType.Savings] = "3"
    };

    public static readonly IReadOnlyDictionary<CardEntryType, string> CardEntryTypeMap = new Dictionary<CardEntryType, string>
    {
        [CardEntryType.Contactless] = "C"
    };

    public static readonly IReadOnlyDictionary<CommsMethodType, string> CommsMethodTypeMap = new Dictionary<CommsMethodType, string>
    {
        [CommsMethodType.X25] = "3"
    };

    public static readonly IReadOnlyDictionary<CurrencyStatus, string> CurrencyStatusMap = new Dictionary<CurrencyStatus, string>
    {
        [CurrencyStatus.AUD] = "0"
    };

    public static readonly IReadOnlyDictionary<PayPassStatus, string> PayPassStatusMap = new Dictionary<PayPassStatus, string>
    {
        [PayPassStatus.PayPassNotUsed] = "0"
    };

    public static readonly IReadOnlyDictionary<ReceiptType, string> ReceiptTypeMap = new Dictionary<ReceiptType, string>
    {
        [ReceiptType.Customer] = "C"
    };

    public static readonly IReadOnlyDictionary<InputType, string> InputTypeMap = new Dictionary<InputType, string>
    {
        [InputType.Normal] = "1"
    };

    public static readonly IReadOnlyDictionary<GraphicCode, string> GraphicCodeMap = new Dictionary<GraphicCode, string>
    {
        [GraphicCode.Card] = "3"
    };

    public static readonly IReadOnlyDictionary<LogonType, string> LogonTypeMap = new Dictionary<LogonType, string>
    {
        [LogonType.RSA] = "4"
    };

    public static readonly IReadOnlyDictionary<SettlementType, string> SettlementTypeMap = new Dictionary<SettlementType, string>
    {
        [SettlementType.LastSettlement] = "L"
    };

    public static readonly IReadOnlyDictionary<StatusType, string> StatusTypeMap = new Dictionary<StatusType, string>
    {
        [StatusType.TerminalAppInfo] = "1"
    };

    public static readonly IReadOnlyDictionary<NetworkType, string> NetworkTypeMap = new Dictionary<NetworkType, string>
    {
        [NetworkType.Dialup] = "2"
    };

    public static readonly IReadOnlyDictionary<KeyHandlingType, string> KeyHandlingSchemeMap = new Dictionary<KeyHandlingType, string>
    {
        [KeyHandlingType.SingleDes] = "1"
    };

    public static readonly IReadOnlyDictionary<TerminalCommsType, string> TerminalCommsTypeMap = new Dictionary<TerminalCommsType, string>
    {
        [TerminalCommsType.Infrared] = "1"
    };

    public static readonly IReadOnlyDictionary<EftTerminalType, string> EftTerminalTypeMap = new Dictionary<EftTerminalType, string>
    {
        [EftTerminalType.IngenicoPX328] = "IngenicoPX328"
    };

    public static readonly IReadOnlyDictionary<QueryCardType, string> QueryCardTypeMap = new Dictionary<QueryCardType, string>
    {
        [QueryCardType.ReadCardAndSelectAccount] = "1"
    };

    public static readonly IReadOnlyDictionary<ReprintType, string> ReprintTypeMap = new Dictionary<ReprintType, string>
    {
        [ReprintType.Reprint] = "1"
    };
}