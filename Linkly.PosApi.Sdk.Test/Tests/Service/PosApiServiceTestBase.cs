using Linkly.PosApi.Sdk.Models;
using Linkly.PosApi.Sdk.Models.Authentication;
using Linkly.PosApi.Sdk.Models.ConfigureMerchant;
using Linkly.PosApi.Sdk.Models.Display;
using Linkly.PosApi.Sdk.Models.Logon;
using Linkly.PosApi.Sdk.Models.QueryCard;
using Linkly.PosApi.Sdk.Models.Receipt;
using Linkly.PosApi.Sdk.Models.ReprintReceipt;
using Linkly.PosApi.Sdk.Models.SendKey;
using Linkly.PosApi.Sdk.Models.Settlement;
using Linkly.PosApi.Sdk.Models.Status;
using Linkly.PosApi.Sdk.Models.Transaction;
using Linkly.PosApi.Sdk.Service;
using Linkly.PosApi.Sdk.UnitTest.Common;
using Linkly.PosApi.Sdk.UnitTest.MockApi;
using Xunit.Abstractions;

namespace Linkly.PosApi.Sdk.UnitTest.Tests.Service;

/// <summary>Request handler for requests which do not use sessions.</summary>
internal delegate void RequestMethod<in TRequest>(TRequest request);

/// <summary>Request handler for POS requests which use sessions.</summary>
internal delegate Guid RequestMethodWithSession<in TRequest>(TRequest request);

public abstract class PosApiServiceTestBase
{
    /// <summary>Request handler for requests with or without session support.</summary>
    private delegate Guid? WrappedRequestMethod<in TRequest>(TRequest request);

    private protected readonly TestOutputLogger Logger;
    private protected readonly Mock<IPosApiEventListener> MockListener = new();
    private protected readonly MockApiHttpMessageHandler MockMessageHandler;
    private protected readonly IPosApiService Service;
    private protected readonly PosApiServiceOptions ServiceOptions = new();

    private protected PosApiServiceTestBase(ITestOutputHelper testOutput)
    {
        Logger = new TestOutputLogger(nameof(PosApiServiceTestBase), testOutput);
        var messageHandlerOptions = new MockApiHttpMessageHandlerOptions(TC.ServiceEndpoint, TC.PairingCreds, TC.PosVendorDetails);
        MockMessageHandler = new MockApiHttpMessageHandler(testOutput, messageHandlerOptions);
        var mockHttpClient = new HttpClient(MockMessageHandler);
        Service = new PosApiService(Logger, mockHttpClient, MockListener.Object, TC.ServiceEndpoint, TC.PosVendorDetails,
            ServiceOptions);
    }

    /// <summary>Perform a pairing request and assert the request completed.</summary>
    private protected async Task<string> PairAsync()
    {
        string? secret = null;
        MockListener
            .Setup(listener => listener.PairingComplete(It.IsAny<PairingRequest>(), It.IsAny<PairingResponse>()))
            .Callback<PairingRequest, PairingResponse>((_, resp) => secret = resp.Secret);

        Service.PairingRequest(RequestRepository.GetPairingRequest());

        await Assertions.WaitForConditionAsync(() => secret is not null, Utils.CreateCancellationToken(), "Pairing failed");

        Assert.NotNull(secret);
        MockListener.Verify(listener => listener.PairingComplete(It.IsAny<PairingRequest>(), It.IsAny<PairingResponse>()), Times.Once);

        return secret;
    }

    private static void ValidatePosApiRequest(PosApiRequest request, MockApi.Models.PosApiRequest mockRequest)
    {
        Assert.Equal(EnumMapper.ReceiptAutoPrintMap[request.ReceiptAutoPrint], mockRequest.ReceiptAutoPrint);
        Assert.Equal(request.CutReceipt ? "1" : "0", mockRequest.CutReceipt);
        Assert.Equal(request.PurchaseAnalysisData, mockRequest.PurchaseAnalysisData);
        Assert.Equal(TC.PosVendorDetails.PosId, mockRequest.PosId);
        Assert.Equal(TC.PosVendorDetails.PosName, mockRequest.PosName);
        Assert.Equal(TC.PosVendorDetails.PosVersion, mockRequest.PosVersion);
    }

    private static void ValidatePosApiResponse(MockApi.Models.PosApiResponse mockResponse, PosApiResponse response)
    {
        Assert.Equal(EnumMapper.ResponseTypeMap.GetKeyFromValue(mockResponse.ResponseType), response.ResponseType);
    }

    private protected static void ValidateDisplayResponse(MockApi.Models.DisplayResponse mockResponse, DisplayResponse response)
    {
        Assert.Equal(mockResponse.NumberOfLines, response.NumberOfLines);
        Assert.Equal(mockResponse.LineLength, response.LineLength);
        Assert.Equal(mockResponse.DisplayText, response.DisplayText);
        Assert.Equal(mockResponse.CancelKeyFlag, response.CancelKeyFlag);
        Assert.Equal(mockResponse.AcceptYesKeyFlag, response.AcceptYesKeyFlag);
        Assert.Equal(mockResponse.DeclineNoKeyFlag, response.DeclineNoKeyFlag);
        Assert.Equal(mockResponse.AuthoriseKeyFlag, response.AuthoriseKeyFlag);
        Assert.Equal(mockResponse.OkKeyFlag, response.OkKeyFlag);
        Assert.Equal(EnumMapper.InputTypeMap.GetKeyFromValue(mockResponse.InputType), response.InputType);
        Assert.Equal(EnumMapper.GraphicCodeMap.GetKeyFromValue(mockResponse.GraphicCode), response.GraphicCode);
        Assert.Equal(mockResponse.PurchaseAnalysisData, response.PurchaseAnalysisData);
    }

    private protected static void ValidateReceiptResponse(MockApi.Models.ReceiptResponse mockResponse, ReceiptResponse response)
    {
        Assert.Equal(EnumMapper.ReceiptTypeMap.GetKeyFromValue(mockResponse.Type), response.ReceiptType);
        Assert.Equal(mockResponse.IsPrePrint, response.IsPrePrint);
        Assert.Equal(mockResponse.ReceiptText, response.ReceiptText);
    }

    private protected static void ValidateTransactionRequest(TransactionRequest request, MockApi.Models.TransactionRequest mockRequest)
    {
        ValidatePosApiRequest(request, mockRequest);
        Assert.Equal(request.Merchant, mockRequest.Merchant);
        Assert.Equal(request.Application, mockRequest.Application);
        Assert.Equal(EnumMapper.TxnTypeMap[request.TxnType], mockRequest.TxnType);
        Assert.Equal(request.EnableTip, mockRequest.EnableTip);
        Assert.Equal(request.TrainingMode, mockRequest.TrainingMode);
        Assert.Equal(request.TxnRef, mockRequest.TxnRef);
        Assert.Equal(EnumMapper.PanSourceMap[request.PanSource], mockRequest.PanSource);
        Assert.Equal(request.CurrencyCode, mockRequest.CurrencyCode);
        Assert.Equal(request.CVV, mockRequest.CVV);
        Assert.Equal(request.DateExpiry, mockRequest.DateExpiry);
        Assert.Equal(request.PAN, mockRequest.PAN);
        Assert.Equal(request.DateExpiry, mockRequest.DateExpiry);
        Assert.Equal(request.Track2, mockRequest.Track2);
        Assert.Equal(EnumMapper.AccountTypeMap[request.AccountType], mockRequest.AccountType);
        Assert.Equal(request.RRN, mockRequest.RRN);
        Assert.Equal(request.CVV, mockRequest.CVV);
    }

    private protected static void ValidateTransactionResponse(MockApi.Models.TransactionResponse mockResponse, TransactionResponse response)
    {
        ValidatePosApiResponse(mockResponse, response);
        Assert.Equal(EnumMapper.TxnTypeMap.GetKeyFromValue(mockResponse.TxnType), response.TxnType);
        Assert.Equal(mockResponse.Merchant, response.Merchant);
        Assert.Equal(mockResponse.CardType, response.CardType);
        Assert.Equal(mockResponse.CardName, response.CardName);
        Assert.Equal(mockResponse.RRN, response.RRN);
        Assert.Equal(mockResponse.DateSettlement, response.DateSettlement);
        Assert.Equal(mockResponse.AmtCash, response.AmountCash);
        Assert.Equal(mockResponse.AmtPurchase, response.Amount);
        Assert.Equal(mockResponse.AmtTip, response.AmountTip);
        Assert.Equal(mockResponse.AuthCode, response.AuthCode);
        Assert.Equal(mockResponse.TxnRef, response.TxnRef);
        Assert.Equal(mockResponse.PAN, response.PAN);
        Assert.Equal(mockResponse.DateExpiry, response.DateExpiry);
        Assert.Equal(mockResponse.Track2, response.Track2);
        Assert.Equal(EnumMapper.AccountTypeMap.GetKeyFromValue(mockResponse.AccountType), response.AccountType);
        Assert.Equal(mockResponse.BalanceReceived, response.BalanceReceived);
        Assert.Equal(mockResponse.AvailableBalance, response.AvailableBalance);
        Assert.Equal(mockResponse.ClearedFundsBalance, response.ClearedFundsBalance);
        Assert.Equal(mockResponse.Date, response.Date);
        Assert.Equal(mockResponse.CatId, response.CatId);
        Assert.Equal(mockResponse.CaId, response.CaId);
        Assert.Equal(mockResponse.Stan, response.Stan);
        Assert.Equal(mockResponse.Success, response.Success);
        Assert.Equal(mockResponse.PurchaseAnalysisData, response.PurchaseAnalysisData);
        Assert.Equal(mockResponse.ResponseCode, response.ResponseCode);
        Assert.Equal(mockResponse.ResponseText, response.ResponseText);
        ValidateTransactionFlags(mockResponse.TxnFlags, response.TxnFlags);

        foreach (var (_, mockReceipt, actualReceipt) in Utils.Zip(mockResponse.Receipts, response.Receipts))
            ValidateReceiptResponse(mockReceipt, actualReceipt);
    }

    private static void ValidateTransactionFlags(MockApi.Models.TxnFlags mockFlags, TransactionFlags flags)
    {
        Assert.Equal(mockFlags.Offline, flags.Offline);
        Assert.Equal(mockFlags.ReceiptPrinted, flags.ReceiptPrinted);
        Assert.Equal(EnumMapper.CardEntryTypeMap.GetKeyFromValue(mockFlags.CardEntry), flags.CardEntry);
        Assert.Equal(EnumMapper.CommsMethodTypeMap.GetKeyFromValue(mockFlags.CommsMethod), flags.CommsMethod);
        Assert.Equal(EnumMapper.CurrencyStatusMap.GetKeyFromValue(mockFlags.Currency), flags.Currency);
        Assert.Equal(EnumMapper.PayPassStatusMap.GetKeyFromValue(mockFlags.PayPass), flags.PayPass);
        Assert.Equal(mockFlags.UndefinedFlag6, flags.UndefinedFlag6);
        Assert.Equal(mockFlags.UndefinedFlag7, flags.UndefinedFlag7);
    }

    private protected static void ValidatePreAuthManipulationRequest(PreAuthManipulationRequest request, MockApi.Models.TransactionRequest mockRequest)
    {
        ValidateTransactionRequest(request, mockRequest);
        Assert.Equal(request.Amount, mockRequest.AmtPurchase);
    }

    private protected static void ValidateLogonRequest(LogonRequest request, MockApi.Models.LogonRequest mockRequest)
    {
        ValidatePosApiRequest(request, mockRequest);
        Assert.Equal(request.Merchant, mockRequest.Merchant);
        Assert.Equal(request.Application, mockRequest.Application);
        Assert.Equal(EnumMapper.LogonTypeMap[request.LogonType], mockRequest.LogonType);
    }

    private protected static void ValidateLogonResponse(MockApi.Models.LogonResponse mockResponse, LogonResponse response)
    {
        ValidatePosApiResponse(mockResponse, response);
        Assert.Equal(mockResponse.PinPadVersion, response.PinPadVersion);
        Assert.Equal(mockResponse.Date, response.Date);
        Assert.Equal(mockResponse.CatId, response.CatId);
        Assert.Equal(mockResponse.CaId, response.CaId);
        Assert.Equal(mockResponse.Stan, response.Stan);
        Assert.Equal(mockResponse.Success, response.Success);
        Assert.Equal(mockResponse.ResponseCode, response.ResponseCode);
        Assert.Equal(mockResponse.ResponseText, response.ResponseText);
    }

    private protected static void ValidateSettlementRequest(SettlementRequest request, MockApi.Models.SettlementRequest mockRequest)
    {
        ValidatePosApiRequest(request, mockRequest);
        Assert.Equal(request.Merchant, mockRequest.Merchant);
        Assert.Equal(request.Application, mockRequest.Application);
        Assert.Equal(EnumMapper.SettlementTypeMap[request.SettlementType], mockRequest.SettlementType);
        Assert.Equal(request.ResetTotals, mockRequest.ResetTotals);
    }

    private protected static void ValidateSettlementResponse(MockApi.Models.SettlementResponse mockResponse, SettlementResponse response)
    {
        ValidatePosApiResponse(mockResponse, response);
        Assert.Equal(mockResponse.Merchant, response.Merchant);
        Assert.Equal(mockResponse.SettlementData, response.SettlementData);
        Assert.Equal(mockResponse.Success, response.Success);
        Assert.Equal(mockResponse.ResponseCode, response.ResponseCode);
        Assert.Equal(mockResponse.ResponseText, response.ResponseText);
    }

    private protected static void ValidateStatusRequest(StatusRequest request, MockApi.Models.StatusRequest mockRequest)
    {
        ValidatePosApiRequest(request, mockRequest);
        Assert.Equal(request.Merchant, mockRequest.Merchant);
        Assert.Equal(request.Application, mockRequest.Application);
        Assert.Equal(EnumMapper.StatusTypeMap[request.StatusType], mockRequest.StatusType);
    }

    private protected static void ValidateStatusResponse(MockApi.Models.StatusResponse mockResponse, StatusResponse response)
    {
        ValidatePinPadOptionFlags(mockResponse.OptionsFlags, response.OptionsFlags);
        Assert.Equal(mockResponse.Aiic, response.Aiic);
        Assert.Equal(mockResponse.NII, response.NII);
        Assert.Equal(mockResponse.CatId, response.CatId);
        Assert.Equal(mockResponse.CaId, response.CaId);
        Assert.Equal(mockResponse.Timeout, response.Timeout);
        Assert.Equal(mockResponse.LoggedOn, response.LoggedOn);
        Assert.Equal(mockResponse.PinPadSerialNumber, response.PinPadSerialNumber);
        Assert.Equal(mockResponse.PinPadVersion, response.PinPadVersion);
        Assert.Equal(mockResponse.BankCode, response.BankCode);
        Assert.Equal(mockResponse.BankDescription, response.BankDescription);
        Assert.Equal(mockResponse.KVC, response.KVC);
        Assert.Equal(mockResponse.SafCount, response.SafCount);
        Assert.Equal(EnumMapper.NetworkTypeMap.GetKeyFromValue(mockResponse.NetworkType), response.NetworkType);
        Assert.Equal(mockResponse.HardwareSerial, response.HardwareSerial);
        Assert.Equal(mockResponse.RetailerName, response.RetailerName);
        Assert.Equal(mockResponse.SafCreditLimit, response.SafCreditLimit);
        Assert.Equal(mockResponse.SafDebitLimit, response.SafDebitLimit);
        Assert.Equal(mockResponse.MaxSaf, response.MaxSaf);
        Assert.Equal(EnumMapper.KeyHandlingSchemeMap.GetKeyFromValue(mockResponse.KeyHandlingScheme), response.KeyHandlingScheme);
        Assert.Equal(mockResponse.CashoutLimit, response.CashoutLimit);
        Assert.Equal(mockResponse.RefundLimit, response.RefundLimit);
        Assert.Equal(mockResponse.CpatVersion, response.CpatVersion);
        Assert.Equal(mockResponse.NameTableVersion, response.NameTableVersion);
        Assert.Equal(EnumMapper.TerminalCommsTypeMap.GetKeyFromValue(mockResponse.TerminalCommsType), response.TerminalCommsType);
        Assert.Equal(mockResponse.CardMisreadCount, response.CardMisreadCount);
        Assert.Equal(mockResponse.TotalMemoryInTerminal, response.TotalMemoryInTerminal);
        Assert.Equal(mockResponse.FreeMemoryInTerminal, response.FreeMemoryInTerminal);
        Assert.Equal(EnumMapper.EftTerminalTypeMap.GetKeyFromValue(mockResponse.EftTerminalType), response.EftTerminalType);
        Assert.Equal(mockResponse.NumAppsInTerminal, response.NumAppsInTerminal);
        Assert.Equal(mockResponse.NumLinesOnDisplay, response.NumLinesOnDisplay);
        Assert.Equal(mockResponse.HardwareInceptionDate, response.HardwareInceptionDate);
        Assert.Equal(mockResponse.Success, response.Success);
        Assert.Equal(mockResponse.Merchant, response.Merchant);
        Assert.Equal(mockResponse.ResponseCode, response.ResponseCode);
        Assert.Equal(mockResponse.ResponseText, response.ResponseText);
    }

    private static void ValidatePinPadOptionFlags(MockApi.Models.PinPadOptionFlags mockFlags, PinPadOptionFlags flags)
    {
        Assert.Equal(mockFlags.Tipping, flags.Tipping);
        Assert.Equal(mockFlags.PreAuth, flags.PreAuth);
        Assert.Equal(mockFlags.Completions, flags.Completions);
        Assert.Equal(mockFlags.CashOut, flags.CashOut);
        Assert.Equal(mockFlags.Refund, flags.Refund);
        Assert.Equal(mockFlags.Balance, flags.Balance);
        Assert.Equal(mockFlags.Deposit, flags.Deposit);
        Assert.Equal(mockFlags.Voucher, flags.Voucher);
        Assert.Equal(mockFlags.Moto, flags.Moto);
        Assert.Equal(mockFlags.AutoCompletion, flags.AutoCompletion);
        Assert.Equal(mockFlags.EFB, flags.EFB);
        Assert.Equal(mockFlags.EMV, flags.EMV);
        Assert.Equal(mockFlags.Training, flags.Training);
        Assert.Equal(mockFlags.Withdrawal, flags.Withdrawal);
        Assert.Equal(mockFlags.Transfer, flags.Transfer);
        Assert.Equal(mockFlags.StartCash, flags.StartCash);
    }

    private protected static void ValidateQueryCardRequest(QueryCardRequest request, MockApi.Models.QueryCardRequest mockRequest)
    {
        ValidatePosApiRequest(request, mockRequest);
        Assert.Equal(request.Merchant, mockRequest.Merchant);
        Assert.Equal(request.Application, mockRequest.Application);
        Assert.Equal(EnumMapper.QueryCardTypeMap[request.QueryCardType], mockRequest.QueryCardType);
    }

    private protected static void ValidateQueryCardResponse(MockApi.Models.QueryCardResponse mockResponse, QueryCardResponse response)
    {
        ValidatePosApiResponse(mockResponse, response);

        Assert.Equal(mockResponse.IsTrack1Available, response.IsTrack1Available);
        Assert.Equal(mockResponse.IsTrack2Available, response.IsTrack2Available);
        Assert.Equal(mockResponse.IsTrack3Available, response.IsTrack3Available);
        Assert.Equal(mockResponse.Track1, response.Track1);
        Assert.Equal(mockResponse.Track2, response.Track2);
        Assert.Equal(mockResponse.Track3, response.Track3);
        Assert.Equal(mockResponse.CardName, response.CardName);
        Assert.Equal(EnumMapper.AccountTypeMap.GetKeyFromValue(mockResponse.AccountType), response.AccountType);
        Assert.Equal(mockResponse.Success, response.Success);
        Assert.Equal(mockResponse.Merchant, response.Merchant);
        Assert.Equal(mockResponse.ResponseCode, response.ResponseCode);
        Assert.Equal(mockResponse.ResponseText, response.ResponseText);
    }

    private protected static void ValidateConfigureMerchantRequest(ConfigureMerchantRequest request, MockApi.Models.ConfigureMerchantRequest mockRequest)
    {
        ValidatePosApiRequest(request, mockRequest);
        Assert.Equal(request.Merchant, mockRequest.Merchant);
        Assert.Equal(request.Application, mockRequest.Application);
        Assert.Equal(request.CatId, mockRequest.CatId);
        Assert.Equal(request.CaId, mockRequest.CaId);
    }

    private protected static void ValidateConfigureMerchantResponse(MockApi.Models.ConfigureMerchantResponse mockResponse, ConfigureMerchantResponse response)
    {
        ValidatePosApiResponse(mockResponse, response);
        Assert.Equal(mockResponse.Merchant, response.Merchant);
        Assert.Equal(mockResponse.Success, response.Success);
        Assert.Equal(mockResponse.ResponseCode, response.ResponseCode);
        Assert.Equal(mockResponse.ResponseText, response.ResponseText);
    }

    private protected static void ValidateReprintReceiptRequest(ReprintReceiptRequest request, MockApi.Models.ReprintReceiptRequest mockRequest)
    {
        ValidatePosApiRequest(request, mockRequest);
        Assert.Equal(request.Merchant, mockRequest.Merchant);
        Assert.Equal(request.Application, mockRequest.Application);
        Assert.Equal(EnumMapper.ReprintTypeMap[request.ReprintType], mockRequest.ReprintType);
    }

    private protected static void ValidateReprintReceiptResponse(MockApi.Models.ReprintReceiptResponse mockResponse, ReprintReceiptResponse response)
    {
        ValidatePosApiResponse(mockResponse, response);

        Assert.Equal(mockResponse.ReceiptText, response.ReceiptText);
        Assert.Equal(mockResponse.Merchant, response.Merchant);
        Assert.Equal(mockResponse.Success, response.Success);
        Assert.Equal(mockResponse.ResponseCode, response.ResponseCode);
        Assert.Equal(mockResponse.ResponseText, response.ResponseText);
    }

    private protected static void ValidateSendKeyRequest(SendKeyRequest request, MockApi.Models.SendKeyRequest mockRequest)
    {
        ValidatePosApiRequest(request, mockRequest);
        Assert.Equal(request.Key, mockRequest.Key);
        Assert.Equal(request.Data, mockRequest.Data);
    }
    private static void ValidateErrorResponseAsync(Exception raisedException, ErrorResponse errorResponse)
    {
        Assert.Equal(ErrorSource.Internal, errorResponse.Source);
        Assert.Null(errorResponse.HttpStatusCode);
        Assert.Equal("Error completing request", errorResponse.Message);
        Assert.Equivalent(raisedException, errorResponse.Exception);
    }

    private static void ValidateErrorResponseAsync(HttpResponseMessage sentHttpErrorMessage, ErrorResponse errorResponse)
    {
        Assert.Equal(ErrorSource.API, errorResponse.Source);
        Assert.Equal(sentHttpErrorMessage.StatusCode, errorResponse.HttpStatusCode);
        Assert.Equal(sentHttpErrorMessage.Content.ReadAsStringAsync().Result, errorResponse.Message);
        Assert.Null(errorResponse.Exception);
    }

    /// <summary>Pair and send a transaction request and assert the listeners are invoked correctly.</summary>
    private protected async Task<(
            MockApi.Models.TransactionRequest MockTransactionRequest,
            MockApi.Models.TransactionResponse MockTransactionResponse,
            Guid SessionId)> 
        SendTransactionRequestAndValidateResponseAsync(TransactionRequest request, bool verifyMockInvocations = true)
    {
        // set up
        List<DisplayResponse> listenerDisplayResponses = new();
        List<ReceiptResponse> listenerReceiptResponses = new();
        TransactionResponse? listenerTransactionResponse = null;

        MockListener
            .Setup(listener => listener.Display(It.IsAny<Guid>(), request, It.IsAny<DisplayResponse>()))
            .Callback<Guid, PosApiRequest, DisplayResponse>((_, _, resp) => listenerDisplayResponses.Add(resp));
        MockListener
            .Setup(listener => listener.Receipt(It.IsAny<Guid>(), request, It.IsAny<ReceiptResponse>()))
            .Callback<Guid, PosApiRequest, ReceiptResponse>((_, _, resp) => listenerReceiptResponses.Add(resp));
        MockListener
            .Setup(listener => listener.TransactionComplete(It.IsAny<Guid>(), request, It.IsAny<TransactionResponse>()))
            .Callback<Guid, TransactionRequest, TransactionResponse>((_, _, resp) => listenerTransactionResponse = resp);

        // execute
        var sessionId = Service.TransactionRequest(request);
        await Assertions.WaitForConditionAsync(() => listenerTransactionResponse is not null, Utils.CreateCancellationToken(),
            $"{nameof(IPosApiEventListener.TransactionComplete)}() listener not invoked");

        // verify
        Assert.NotNull(listenerTransactionResponse);
        Assert.True(MockMessageHandler.Session.Requests.TryGetSession(sessionId, out var mockBaseRequests, out var mockBaseResponses));

        var mockRequest = Assertions.SingleOfType<MockApi.Models.TransactionRequest>(mockBaseRequests);
        var mockResponse = Assertions.SingleOfType<MockApi.Models.TransactionResponse>(mockBaseResponses);

        ValidateTransactionRequest(request, mockRequest);
        ValidateTransactionResponse(mockResponse, listenerTransactionResponse);

        var mockReceiptResponses = mockBaseResponses.OfType<MockApi.Models.ReceiptResponse>();
        var mockDisplayResponses = mockBaseResponses.OfType<MockApi.Models.DisplayResponse>();
        foreach (var (_, listenerReceiptResponse, mockReceiptResponse) in Utils.Zip(listenerReceiptResponses, mockReceiptResponses))
            ValidateReceiptResponse(mockReceiptResponse, listenerReceiptResponse);
        foreach (var (_, listenerDisplayResponse, mockDisplayResponse) in Utils.Zip(listenerDisplayResponses, mockDisplayResponses))
            ValidateDisplayResponse(mockDisplayResponse, listenerDisplayResponse);

        if (verifyMockInvocations)
        {
            Assert.InRange(MockMessageHandler.Session.NumTokenRequests, 1, int.MaxValue);
            MockListener.Verify(listener => listener.Display(sessionId, It.IsAny<TransactionRequest>(), It.IsAny<DisplayResponse>()),
                Times.Exactly(3));
            MockListener.Verify(listener => listener.Receipt(sessionId, It.IsAny<TransactionRequest>(), It.IsAny<ReceiptResponse>()), Times.Once);
            MockListener.Verify(listener => listener.TransactionComplete(sessionId, It.IsAny<TransactionRequest>(), It.IsAny<TransactionResponse>()),
                Times.Once);
            MockListener.VerifyNoOtherCalls();
        }

        return (mockRequest, mockResponse, sessionId);
    }

    /// <summary>
    /// Perform an API request and assert the <see cref="IPosApiEventListener.Error" /> listener is
    /// correctly invoked. Precondition: <see cref="MockMessageHandler" /> has been configured to throw
    /// <see cref="expectedException" />.
    /// </summary>
    /// <typeparam name="TRequest">Request to send.</typeparam>
    /// <param name="requestMethod">Request method to invoke.</param>
    /// <param name="request">Request body to send to <see cref="requestMethod" />.</param>
    /// <param name="expectedException">Exception to verify is sent by the
    /// <see cref="IPosApiEventListener.Error" /> listener.
    /// </param>
    private protected async Task SendRequestAndValidateErrorResponseAsync<TRequest>(RequestMethod<TRequest> requestMethod,
        TRequest request,
        Exception expectedException)
        where TRequest : IBaseRequest => await SendRequestAndValidateErrorResponseAsync(WrapRequestMethod(requestMethod), request, expectedException);

    /// <inheritdoc cref="SendRequestAndValidateErrorResponseAsync{TRequest}(RequestMethod{TRequest},TRequest,Exception)" />
    private protected async Task SendRequestAndValidateErrorResponseAsync<TRequest>(RequestMethodWithSession<TRequest> requestMethod,
        TRequest request,
        Exception expectedException)
        where TRequest : PosApiRequest => await SendRequestAndValidateErrorResponseAsync(WrapRequestMethod(requestMethod), request, expectedException);

    private async Task SendRequestAndValidateErrorResponseAsync<TRequest>(WrappedRequestMethod<TRequest> requestMethod,
        TRequest request,
        Exception expectedException)
        where TRequest : IBaseRequest
    {
        var listenerErrorResponse = await SendRequestAndAwaitErrorResponse(requestMethod, request);
        ValidateErrorResponseAsync(expectedException, listenerErrorResponse);
    }

    /// <summary>
    /// Perform an API request and assert the <see cref="IPosApiEventListener.Error" /> listener is invoked
    /// and the error response is correct. Precondition: <see cref="MockMessageHandler" /> has been
    /// configured to return a <see cref="expectedHttpError" />.
    /// </summary>
    /// <typeparam name="TRequest">Request to send.</typeparam>
    /// <param name="requestMethod">Request method to invoke.</param>
    /// <param name="request">Request body to send to <see cref="requestMethod" />.</param>
    /// <param name="expectedHttpError">
    /// HTTP error response to verify is sent by the
    /// <see cref="IPosApiEventListener.Error" /> listener.
    /// </param>
    private protected async Task SendRequestAndValidateErrorResponseAsync<TRequest>(RequestMethod<TRequest> requestMethod,
        TRequest request,
        HttpResponseMessage expectedHttpError)
        where TRequest : IBaseRequest => await SendRequestAndValidateErrorResponseAsync(WrapRequestMethod(requestMethod), request, expectedHttpError);

    /// <inheritdoc cref="SendRequestAndValidateErrorResponseAsync{TRequest}(RequestMethod{TRequest},TRequest,Exception)" />
    private protected async Task SendRequestAndValidateErrorResponseAsync<TRequest>(RequestMethodWithSession<TRequest> requestMethod,
        TRequest request, HttpResponseMessage expectedHttpError)
        where TRequest : PosApiRequest => await SendRequestAndValidateErrorResponseAsync(WrapRequestMethod(requestMethod), request, expectedHttpError);

    private async Task SendRequestAndValidateErrorResponseAsync<TRequest>(WrappedRequestMethod<TRequest> requestMethod,
        TRequest request, HttpResponseMessage expectedHttpError)
        where TRequest : IBaseRequest
    {
        var listenerErrorResponse = await SendRequestAndAwaitErrorResponse(requestMethod, request);
        ValidateErrorResponseAsync(expectedHttpError, listenerErrorResponse);
    }

    /// <summary>
    /// Send a request with an error condition set and assert the <see cref="IPosApiEventListener.Error" />
    /// listener is invoked.
    /// </summary>
    private async Task<ErrorResponse> SendRequestAndAwaitErrorResponse<TRequest>(WrappedRequestMethod<TRequest> requestMethod, TRequest request)
        where TRequest : IBaseRequest
    {
        ErrorResponse? errorResponse = null;

        MockListener
            .Setup(listener => listener.Error(It.IsAny<Guid?>(), request, It.IsAny<ErrorResponse>()))
            .Callback<Guid?, IBaseRequest, ErrorResponse>((_, _, resp) => errorResponse = resp);

        var sessionId = requestMethod(request);
        await Assertions.WaitForConditionAsync(() => errorResponse is not null, Utils.CreateCancellationToken(),
            $"{nameof(IPosApiEventListener.Error)}() listener not invoked");

        Assert.NotNull(errorResponse);
        MockListener.Verify(listener => listener.Error(sessionId, It.IsAny<IBaseRequest>(), It.IsAny<ErrorResponse>()), Times.Once);
        MockListener.VerifyNoOtherCalls();

        return errorResponse;
    }

    private static WrappedRequestMethod<TRequest> WrapRequestMethod<TRequest>(RequestMethod<TRequest> requestMethod)
    {
        Guid? WrappedRequestMethod(TRequest req)
        {
            requestMethod(req);
            return null;
        }

        return WrappedRequestMethod;
    }

    private static WrappedRequestMethod<TRequest> WrapRequestMethod<TRequest>(RequestMethodWithSession<TRequest> requestMethod) =>
        req => requestMethod(req);
}