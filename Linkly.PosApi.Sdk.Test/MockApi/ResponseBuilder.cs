using Linkly.PosApi.Sdk.UnitTest.Common;
using Linkly.PosApi.Sdk.UnitTest.MockApi.Models;

namespace Linkly.PosApi.Sdk.UnitTest.MockApi;

/// <summary>Build mock response objects for all types of requests.</summary>
internal static class ResponseBuilder
{
    public static DisplayResponse GetDisplayResponse(string displayText) => new()
    {
        NumberOfLines = 1,
        DisplayText = new List<string> { displayText },
        ResponseType = "display",
        AcceptYesKeyFlag = true,
        AuthoriseKeyFlag = true,
        CancelKeyFlag = true,
        DeclineNoKeyFlag = true,
        OkKeyFlag = true,
        InputType = "1",
        GraphicCode = "3",
        LineLength = 1,
        PurchaseAnalysisData = new Dictionary<string, string>
        {
            ["DISPLAY"] = "Test"
        }
    };

    public static ReceiptResponse GetReceiptResponse(string displayText) => new()
    {
        Type = "C",
        ReceiptText = new List<string> { displayText },
        ResponseType = "receipt"
    };

    public static TransactionResponse GetTransactionResponse(TransactionRequest request) => new()
    {
        Merchant = request.Merchant,
        CardType = "AMEX CARD".PadRight(20),
        CardName = "05",
        TxnType = request.TxnType,
        AmtPurchase = request.AmtPurchase ?? 0,
        AmtCash = request.AmtCash ?? 0,
        AmtTip = 10,
        AccountType = request.AccountType,
        TxnRef = request.TxnRef,
        DateExpiry = request.DateExpiry ?? "0140",
        PAN = request.PAN ?? "109481726172",
        AuthCode = request.AuthCode ?? 0,
        CaId = Utils.GenRandomString(15, false),
        CatId = Utils.GenRandomString(8, false),
        Stan = 11,
        RRN = request.RRN,
        ResponseCode = "00",
        ResponseText = "APPROVED".PadRight(20),
        Success = true,
        DateSettlement = DateTime.Now.Date,
        Date = DateTime.Now,
        TxnFlags = new TxnFlags
        {
            Offline = "0",
            ReceiptPrinted = "1",
            CardEntry = "C",
            CommsMethod = "3",
            Currency = "0",
            PayPass = "0",
            UndefinedFlag6 = "0",
            UndefinedFlag7 = "0"
        },
        PurchaseAnalysisData = new Dictionary<string, string>
        {
            ["AMT"] = (request.AmtPurchase ?? 0).ToString().PadLeft(9, '0')
        },
        Track2 = "12345678".PadRight(40),
        Receipts = new List<ReceiptResponse>
        {
            GetReceiptResponse("Txn Receipt 1"),
            GetReceiptResponse("Txn Receipt 2")
        },
        ResponseType = "transaction"
    };

    public static LogonResponse GetLogonResponse() => new()
    {
        PinPadVersion = "100800".PadRight(16),
        Success = true,
        ResponseCode = "00",
        ResponseText = "APPROVED".PadRight(20),
        Stan = 11,
        CaId = Utils.GenRandomString(15, false),
        CatId = Utils.GenRandomString(8, false),
        ResponseType = "logon",
        Date = DateTime.Now
    };

    public static SettlementResponse GetSettlementResponse() => new()
    {
        Merchant = "00",
        Success = true,
        SettlementData = "2837D827389273K82378273A",
        ResponseCode = "00",
        ResponseText = "APPROVED".PadRight(20),
        ResponseType = "settlement"
    };

    public static StatusResponse GetStatusResponse() => new()
    {
        Merchant = "00",
        Aiic = "444777".PadRight(11),
        NII = 1,
        CaId = Utils.GenRandomString(15, false),
        CatId = Utils.GenRandomString(8, false),
        Timeout = 45,
        LoggedOn = true,
        PinPadSerialNumber = "123456789012345A",
        PinPadVersion = "120800".PadRight(16),
        BankCode = "6",
        BankDescription = "PC-EFTPOS OFFLINE VPP".PadRight(32),
        KVC = "1",
        SafCount = 1,
        NetworkType = "2",
        HardwareSerial = "3984239019825690",
        RetailerName = "TEST MERCHANT NAME  LINE2".PadRight(50),
        OptionsFlags = new PinPadOptionFlags
        {
            Tipping = true,
            PreAuth = true,
            Completions = true,
            CashOut = true,
            Refund = true,
            Balance = true,
            Deposit = true,
            Voucher = true,
            Moto = true,
            AutoCompletion = true,
            EFB = true,
            EMV = true,
            Training = true,
            Withdrawal = true,
            Transfer = true,
            StartCash = true
        },
        SafCreditLimit = 100,
        SafDebitLimit = 100,
        MaxSaf = 999,
        KeyHandlingScheme = "1",
        CashoutLimit = 99999900,
        RefundLimit = 99999900,
        CpatVersion = "000100",
        NameTableVersion = "000100",
        TerminalCommsType = "1",
        CardMisreadCount = 1,
        TotalMemoryInTerminal = 1000,
        FreeMemoryInTerminal = 300,
        EftTerminalType = "IngenicoPX328",
        NumAppsInTerminal = 10,
        NumLinesOnDisplay = 20,
        HardwareInceptionDate = DateTime.Now,
        Success = true,
        ResponseCode = "T0",
        ResponseText = "APPROVED".PadRight(20),
        ResponseType = "status"
    };

    public static QueryCardResponse GetQueryCardResponse() => new()
    {
        Merchant = "00",
        IsTrack1Available = true,
        IsTrack2Available = true,
        IsTrack3Available = true,
        Track1 = "28372837".PadRight(40),
        Track2 = "38247329874".PadRight(40),
        Track3 = "239829381".PadRight(40),
        CardName = "32",
        AccountType = "2",
        Success = true,
        ResponseCode = "T0",
        ResponseText = "APPROVED".PadRight(20),
        PurchaseAnalysisData = new Dictionary<string, string> { ["CEM"] = "S" },
        ResponseType = "querycard"
    };

    public static ConfigureMerchantResponse GetConfigureMerchantResponse() => new()
    {
        Merchant = "00",
        Success = true,
        ResponseCode = "00",
        ResponseText = "APPROVED".PadRight(20),
        ResponseType = "configuremerchant"
    };

    public static ReprintReceiptResponse GetReprintReceiptResponse() => new()
    {
        ResponseText = "APPROVED".PadRight(20),
        ResponseCode = "00",
        Success = true,
        Merchant = "00",
        ReceiptText = new List<string> { "Receipt" },
        ResponseType = "reprintreceipt"
    };
}