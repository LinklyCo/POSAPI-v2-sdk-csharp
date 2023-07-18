using Linkly.PosApi.Sdk.Models;
using Linkly.PosApi.Sdk.Models.Authentication;
using Linkly.PosApi.Sdk.Models.ConfigureMerchant;
using Linkly.PosApi.Sdk.Models.Logon;
using Linkly.PosApi.Sdk.Models.QueryCard;
using Linkly.PosApi.Sdk.Models.ReprintReceipt;
using Linkly.PosApi.Sdk.Models.Result;
using Linkly.PosApi.Sdk.Models.SendKey;
using Linkly.PosApi.Sdk.Models.Settlement;
using Linkly.PosApi.Sdk.Models.Status;
using Linkly.PosApi.Sdk.Models.Transaction;
using Linkly.PosApi.Sdk.Service;

namespace Linkly.PosApi.Sdk.UnitTest.Common;

/// <summary>Repository for generating sample requests for the <see cref="PosApiService" />.</summary>
internal static class RequestRepository
{
    public static PairingRequest GetPairingRequest() => new()
    {
        Username = TC.PairingCreds.Username,
        Password = TC.PairingCreds.Password,
        PairCode = TC.PairingCreds.PairCode
    };

    public static PurchaseRequest GetPurchaseRequest()
    {
        var request = new PurchaseRequest()
        {
            Amount = 10000,
            AmountCash = 5000,
            CurrencyCode = "AUD",
            AccountType = AccountType.Credit,
            EnableTip = true,
            CutReceipt = true,
            ReceiptAutoPrint = ReceiptAutoPrint.PinPad,
            TrainingMode = true,
            TxnRef = Utils.GenTxnRef(),
            AuthCode = 123,
            Track2 = Utils.GenTrack2(),
            PanSource = PanSource.PosKeyed,
            DateExpiry = "0122",
            PAN = Utils.GenPan(),
            RRN = Utils.GenRrn(),
            CVV = 123,
            TipAmount = 1234,
            ProductLevelBlock = false
        };

        var surchargeOptions = new SurchargeOptions();
        surchargeOptions.Add(new FixedSurcharge("45", 125));
        request.SetSurchargeOptions(surchargeOptions);

        request.SetTipOptions(new TippingOptions(5, 10, 15));

        return request;
    }

    public static RefundRequest GetRefundRequest() => new()
    {
        Amount = 10000,
        CurrencyCode = "AUD",
        AccountType = AccountType.Credit,
        EnableTip = true,
        CutReceipt = true,
        ReceiptAutoPrint = ReceiptAutoPrint.PinPad,
        TrainingMode = true,
        TxnRef = Utils.GenTxnRef(),
        AuthCode = 123,
        Track2 = Utils.GenTrack2(),
        PanSource = PanSource.PosKeyed,
        DateExpiry = "0122",
        PAN = Utils.GenPan(),
        RRN = Utils.GenRrn(),
        CVV = 123
    };

    public static CashRequest GetCashRequest() => new()
    {
        AmountCash = 50000,
        CurrencyCode = "AUD",
        AccountType = AccountType.Savings,
        EnableTip = true,
        CutReceipt = true,
        ReceiptAutoPrint = ReceiptAutoPrint.PinPad,
        TrainingMode = true,
        TxnRef = Utils.GenTxnRef(),
        AuthCode = 123,
        Track2 = Utils.GenTrack2(),
        PanSource = PanSource.PinPad,
        RRN = Utils.GenRrn()
    };

    public static DepositRequest GetDepositRequest() => new()
    {
        AmountCash = 150000,
        AmountCheque = 50095,
        TotalCheques = 5,
        CurrencyCode = "AUD",
        AccountType = AccountType.Savings,
        EnableTip = true,
        CutReceipt = true,
        ReceiptAutoPrint = ReceiptAutoPrint.PinPad,
        TrainingMode = true,
        TxnRef = Utils.GenTxnRef(),
        AuthCode = 123,
        Track2 = Utils.GenTrack2(),
        PanSource = PanSource.PinPad,
        RRN = Utils.GenRrn()
    };

    public static VoidRequest GetVoidRequest() => new()
    {
        CurrencyCode = "AUD",
        TxnRef = Utils.GenTxnRef(),
        Amount = 10000
    };

    public static PreAuthRequest GetPreAuthRequest() => new()
    {
        Amount = 10000,
        TxnRef = Utils.GenTxnRef(),
    };

    public static PreAuthCancelRequest GetPreAuthCancelRequest() => new()
    {
        RFN = Utils.GenRfn(),
        TxnRef = Utils.GenTxnRef(),
    };

    public static PreAuthCompletionRequest GetPreAuthCompletionRequest() => new()
    {
        Amount = 10000,
        RFN = Utils.GenRfn(),
        TxnRef = Utils.GenTxnRef(),
    };

    public static PreAuthExtendRequest GetPreAuthExtendRequest() => new()
    {
        RFN = Utils.GenRfn(),
        TxnRef = Utils.GenTxnRef(),
    };

    public static PreAuthTopUpRequest GetPreAuthTopUpRequest() => new()
    {
        Amount = 50000,
        RFN = Utils.GenRfn(),
        TxnRef = Utils.GenTxnRef(),
    };

    public static PreAuthPartialCancelRequest GetPreAuthPartialCancelRequest() => new()
    {
        Amount = 50000,
        RFN = Utils.GenRfn(),
        TxnRef = Utils.GenTxnRef(),
    };

    public static PreAuthSummaryRequest GetPreAuthSummaryRequest() => new()
    {
        PreAuthIndex = 5,
        TxnRef = Utils.GenTxnRef()
    };

    public static PreAuthInquiryRequest GetPreAuthInquiryRequest() => new()
    {
        RFN = Utils.GenRfn(),
        TxnRef = Utils.GenTxnRef()
    };

    public static LogonRequest GetLogonRequest() => new()
    {
        LogonType = LogonType.RSA,
        Merchant = "15",
        Application = "81",
        ReceiptAutoPrint = ReceiptAutoPrint.Both
    };

    public static SettlementRequest GetSettlementRequest() => new()
    {
        SettlementType = SettlementType.LastSettlement,
        ResetTotals = true,
        Merchant = "15",
        Application = "81"
    };

    public static StatusRequest GetStatusRequest() => new()
    {
        StatusType = StatusType.TerminalAppInfo
    };

    public static QueryCardRequest GetQueryCardRequest() => new()
    {
        QueryCardType = QueryCardType.ReadCardAndSelectAccount
    };

    public static ConfigureMerchantRequest GetConfigureMerchantRequest() => new()
    {
        CatId = "92389238",
        CaId = "893489734"
    };

    public static ReprintReceiptRequest GetReprintReceiptRequest() => new()
    {
        ReprintType = ReprintType.Reprint
    };

    public static SendKeyRequest GetSendKeyRequest(Guid sessionId) => new()
    {
        SessionId = sessionId,
        Key = "1",
        Data = "Abc123"
    };

    public static ResultRequest GetResultRequest(Guid sessionId) => new(sessionId);
}