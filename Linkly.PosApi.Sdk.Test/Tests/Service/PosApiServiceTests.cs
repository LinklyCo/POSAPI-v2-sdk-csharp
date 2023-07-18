using System.Net;
using Linkly.PosApi.Sdk.Models;
using Linkly.PosApi.Sdk.Models.Authentication;
using Linkly.PosApi.Sdk.Models.ConfigureMerchant;
using Linkly.PosApi.Sdk.Models.Display;
using Linkly.PosApi.Sdk.Models.Logon;
using Linkly.PosApi.Sdk.Models.QueryCard;
using Linkly.PosApi.Sdk.Models.Receipt;
using Linkly.PosApi.Sdk.Models.ReprintReceipt;
using Linkly.PosApi.Sdk.Models.Result;
using Linkly.PosApi.Sdk.Models.SendKey;
using Linkly.PosApi.Sdk.Models.Settlement;
using Linkly.PosApi.Sdk.Models.Status;
using Linkly.PosApi.Sdk.Models.Transaction;
using Linkly.PosApi.Sdk.Service;
using Linkly.PosApi.Sdk.UnitTest.Common;
using Linkly.PosApi.Sdk.UnitTest.TestData;
using Xunit.Abstractions;

namespace Linkly.PosApi.Sdk.UnitTest.Tests.Service;

public class PosApiServiceTests : PosApiServiceTestBase
{
    public PosApiServiceTests(ITestOutputHelper testOutput) : base(testOutput)
    {
    }

    [Fact]
    public void Initialise_EventListenerNull_ThrowsArgumentNullException() =>
        Assert.Throws<ArgumentNullException>(() => new PosApiService(null, null, null!, TC.ServiceEndpoint, TC.PosVendorDetails));

    [Fact]
    public void Initialise_ServiceEndpointNull_ThrowsArgumentNullException() =>
        Assert.Throws<ArgumentNullException>(() => new PosApiService(null, null, MockListener.Object, null!, TC.PosVendorDetails));

    [Fact]
    public void Initialise_PosVendorDetailsNull_ThrowsArgumentNullException() =>
        Assert.Throws<ArgumentNullException>(() => new PosApiService(null, null, MockListener.Object, TC.ServiceEndpoint, null!));

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void SetPairSecret_ChangeSecret_PerformsNewTokenRequest(string? pairSecret) =>
        Assert.Throws<ArgumentException>(() => Service.SetPairSecret(pairSecret!));

    [Fact]
    public async Task SetPairSecret_ChangeSecret_PerformsAdditionalTokenRequest()
    {
        var secret = await PairAsync();

        await SendTransactionRequestAndValidateResponseAsync(RequestRepository.GetPurchaseRequest(), false);

        // Changing the secret should clear the auth token. Then change back to real token.
        Service.SetPairSecret("Fake");
        Service.SetPairSecret(secret);

        await SendTransactionRequestAndValidateResponseAsync(RequestRepository.GetPurchaseRequest(), false);

        Assert.Equal(2, MockMessageHandler.Session.NumTokenRequests);
    }

    [Fact]
    public async Task SetPairSecret_SameSecretSet_DoesNotPerformAdditionalTokenRequest()
    {
        var secret = await PairAsync();

        await SendTransactionRequestAndValidateResponseAsync(RequestRepository.GetPurchaseRequest(), false);

        // Since we are setting the same secret already in use, this should not clear the auth token.
        Service.SetPairSecret(secret);

        await SendTransactionRequestAndValidateResponseAsync(RequestRepository.GetPurchaseRequest(), false);

        Assert.Equal(1, MockMessageHandler.Session.NumTokenRequests);
    }

    [Fact]
    public async Task SetEventListener_SwitchEventListener_PairingRequestOnlyInvokedOnNewListener()
    {
        var completed = false;
        var mockListener = new Mock<IPosApiEventListener>();
        mockListener
            .Setup(listener => listener.PairingComplete(It.IsAny<PairingRequest>(), It.IsAny<PairingResponse>()))
            .Callback(() => completed = true);

        Service.SetEventListener(mockListener.Object);
        Service.PairingRequest(RequestRepository.GetPairingRequest());

        await Assertions.WaitForConditionAsync(() => completed, Utils.CreateCancellationToken(),
            $"{nameof(IPosApiEventListener.PairingComplete)}() listener not invoked");

        mockListener.Verify(listener => listener.PairingComplete(It.IsAny<PairingRequest>(), It.IsAny<PairingResponse>()), Times.Once);
        MockListener.Verify(listener => listener.PairingComplete(It.IsAny<PairingRequest>(), It.IsAny<PairingResponse>()), Times.Never);
        mockListener.VerifyNoOtherCalls();
        MockListener.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task PairingRequest_RequestValid_VerifyRequestAndResponse()
    {
        PairingResponse? listenerResponse = null;
        var request = RequestRepository.GetPairingRequest();
        MockListener
            .Setup(listener => listener.PairingComplete(request, It.IsAny<PairingResponse>()))
            .Callback<PairingRequest, PairingResponse>((_, resp) => listenerResponse = resp);

        Service.PairingRequest(request);
        await Assertions.WaitForConditionAsync(() => listenerResponse is not null, Utils.CreateCancellationToken(),
            $"{nameof(IPosApiEventListener.PairingComplete)}() listener not invoked");

        Assert.NotNull(listenerResponse);
        var session = Assert.Single(MockMessageHandler.Session.Requests);
        var mockRequest = Assertions.SingleOfType<MockApi.Models.PairingRequest>(session.Requests);
        var mockResponse = Assertions.SingleOfType<MockApi.Models.PairingResponse>(session.Responses);
        // check request is serialised correctly
        Assert.Equal(request.Username, mockRequest.Username);
        Assert.Equal(request.PairCode, mockRequest.PairCode);
        Assert.Equal(request.Password, mockRequest.Password);
        // check response is deserialised correctly
        Assert.Equal(mockResponse.Secret, listenerResponse.Secret);
        MockListener.Verify(listener => listener.PairingComplete(It.IsAny<PairingRequest>(), It.IsAny<PairingResponse>()), Times.Once);
        MockListener.VerifyNoOtherCalls();
    }

    [Theory]
    [ClassData(typeof(PairingRequestNegativeTestDataBuilder))]
    public void PairingRequest_RequestContainsValidationError_ThrowsArgumentException(PairingRequest request, string invalidParam)
    {
        var ex = Assert.Throws<ArgumentException>(() => Service.PairingRequest(request));

        Assert.Contains(invalidParam, ex.Message);
    }

    [Fact]
    public async Task PairingRequest_UnauthorizedErrorResponseSendingPairingRequest_ErrorListenerInvoked()
    {
        var requestError = MockMessageHandler.Options.PairRequestError = new HttpResponseMessage(HttpStatusCode.Unauthorized)
            { Content = new StringContent("Unauthorised") };

        await SendRequestAndValidateErrorResponseAsync(Service.PairingRequest, RequestRepository.GetPairingRequest(), requestError);
    }

    [Fact]
    public async Task PairingRequest_HttpRequestExceptionThrownSendingPairingRequest_ErrorListenerInvoked()
    {
        var requestException = MockMessageHandler.Options.PairRequestException = new HttpRequestException();

        await SendRequestAndValidateErrorResponseAsync(Service.PairingRequest, RequestRepository.GetPairingRequest(), requestException);
    }

    [Theory]
    [InlineData(false, false, false)]
    [InlineData(true, false, false)]
    [InlineData(true, true, false)]
    [InlineData(true, false, true)]
    public async Task PurchaseRequest_RequestValid_VerifyRequestAndResponse(bool delayPinPadReply, bool resultTimesOutImmediately, bool aggressiveTokenLeasePeriod)
    {
        // This configures an artificial delay between each event callback so when the result endpoint
        // is queried it will not pickup all the events in the first iteration.
        if (delayPinPadReply)
            MockMessageHandler.Options.PinPadDelay = TimeSpan.FromMilliseconds(100);
        // This configures how long the result endpoint should poll for new events before returning a
        // HTTP 425 (Too late) response.
        if (resultTimesOutImmediately)
            MockMessageHandler.Options.ResultTimeout = TimeSpan.Zero;
        // This configures how long the auth token is valid for to check the SDK is able to renew
        // the token transparently.
        if (aggressiveTokenLeasePeriod)
            MockMessageHandler.Options.TokenLeasePeriod = TimeSpan.FromMilliseconds(100);

        await PairAsync();
        var request = RequestRepository.GetPurchaseRequest();

        var (mockRequest, _, _) = await SendTransactionRequestAndValidateResponseAsync(request);

        Assert.Equal(TxnType.Purchase, request.TxnType);
        Assert.Equal(request.Amount, mockRequest.AmtPurchase);
        Assert.Equal(request.AmountCash, mockRequest.AmtCash);

        if (aggressiveTokenLeasePeriod)
            Assert.InRange(MockMessageHandler.Session.NumTokenRequests, 2, int.MaxValue);
        else
            Assert.Equal(1, MockMessageHandler.Session.NumTokenRequests);
    }

    [Theory]
    [ClassData(typeof(PurchaseRequestNegativeTestDataBuilder))]
    public void PurchaseRequest_RequestContainsValidationError_ThrowsArgumentException(PurchaseRequest request, string invalidParam)
    {
        Service.SetPairSecret("123");

        var ex = Assert.Throws<ArgumentException>(() => Service.TransactionRequest(request));

        Assert.Contains(invalidParam, ex.Message);
    }

    [Fact]
    public async Task RefundRequest_RequestValid_VerifyRequestAndResponse()
    {
        await PairAsync();
        var request = RequestRepository.GetRefundRequest();

        var (mockRequest, _, _) = await SendTransactionRequestAndValidateResponseAsync(request);

        Assert.Equal(TxnType.Refund, request.TxnType);
        Assert.Equal(request.Amount, mockRequest.AmtPurchase);
    }

    [Theory]
    [ClassData(typeof(RefundRequestNegativeTestDataBuilder))]
    public void RefundRequest_RequestContainsValidationError_ThrowsArgumentException(RefundRequest request, string invalidParam)
    {
        Service.SetPairSecret("123");

        var ex = Assert.Throws<ArgumentException>(() => Service.TransactionRequest(request));

        Assert.Contains(invalidParam, ex.Message);
    }

    [Fact]
    public async Task CashRequest_RequestValid_VerifyRequestAndResponse()
    {
        await PairAsync();
        var request = RequestRepository.GetCashRequest();

        var (mockRequest, _, _) = await SendTransactionRequestAndValidateResponseAsync(request);

        Assert.Equal(TxnType.Cash, request.TxnType);
        Assert.Equal(request.AmountCash, mockRequest.AmtCash);
    }

    [Theory]
    [ClassData(typeof(CashRequestNegativeTestDataBuilder))]
    public void CashRequest_RequestContainsValidationError_ThrowsArgumentException(CashRequest request, string invalidParam)
    {
        Service.SetPairSecret("123");

        var ex = Assert.Throws<ArgumentException>(() => Service.TransactionRequest(request));

        Assert.Contains(invalidParam, ex.Message);
    }

    [Fact]
    public async Task DepositRequest_RequestValid_VerifyRequestAndResponse()
    {
        await PairAsync();
        var request = RequestRepository.GetDepositRequest();

        var (mockRequest, _, _) = await SendTransactionRequestAndValidateResponseAsync(request);

        Assert.Equal(TxnType.Deposit, request.TxnType);
        Assert.Equal(request.AmountCash, mockRequest.AmtCash);
    }

    [Theory]
    [ClassData(typeof(DepositRequestNegativeTestDataBuilder))]
    public void DepositRequest_RequestContainsValidationError_ThrowsArgumentException(DepositRequest request, string invalidParam)
    {
        Service.SetPairSecret("123");

        var ex = Assert.Throws<ArgumentException>(() => Service.TransactionRequest(request));

        Assert.Contains(invalidParam, ex.Message);
    }

    [Fact]
    public async Task VoidRequest_RequestValid_VerifyRequestAndResponse()
    {
        await PairAsync();
        var request = RequestRepository.GetVoidRequest();

        await SendTransactionRequestAndValidateResponseAsync(request);

        Assert.Equal(TxnType.Void, request.TxnType);
    }

    [Theory]
    [ClassData(typeof(VoidRequestNegativeTestDataBuilder))]
    public void VoidRequest_RequestContainsValidationError_ThrowsArgumentException(VoidRequest request, string invalidParam)
    {
        Service.SetPairSecret("123");

        var ex = Assert.Throws<ArgumentException>(() => Service.TransactionRequest(request));

        Assert.Contains(invalidParam, ex.Message);
    }

    [Fact]
    public async Task PreAuthRequest_RequestValid_VerifyRequestAndResponse()
    {
        await PairAsync();
        var request = RequestRepository.GetPreAuthRequest();

        var (mockRequest, _, _) = await SendTransactionRequestAndValidateResponseAsync(request);

        Assert.Equal(TxnType.PreAuth, request.TxnType);
        Assert.Equal(request.Amount, mockRequest.AmtPurchase);
    }

    [Theory]
    [ClassData(typeof(PreAuthRequestNegativeTestDataBuilder))]
    public void PreAuthRequest_RequestContainsValidationError_ThrowsArgumentException(PreAuthRequest request, string invalidParam)
    {
        Service.SetPairSecret("123");

        var ex = Assert.Throws<ArgumentException>(() => Service.TransactionRequest(request));

        Assert.Contains(invalidParam, ex.Message);
    }

    [Fact]
    public async Task PreAuthCancelRequest_RequestValid_VerifyRequestAndResponse()
    {
        await PairAsync();
        var request = RequestRepository.GetPreAuthCancelRequest();

        var (mockRequest, _, _) = await SendTransactionRequestAndValidateResponseAsync(request);

        Assert.Equal(TxnType.PreAuthCancel, request.TxnType);
        ValidateTransactionRequest(request, mockRequest);
    }

    [Theory]
    [ClassData(typeof(PreAuthCancelRequestNegativeTestDataBuilder))]
    public void PreAuthCancelRequest_RequestContainsValidationError_ThrowsArgumentException(PreAuthCancelRequest request,
        string invalidParam)
    {
        Service.SetPairSecret("123");

        var ex = Assert.Throws<ArgumentException>(() => Service.TransactionRequest(request));

        Assert.Contains(invalidParam, ex.Message);
    }

    [Fact]
    public async Task PreAuthCompletionRequest_RequestValid_VerifyRequestAndResponse()
    {
        await PairAsync();
        var request = RequestRepository.GetPreAuthCompletionRequest();

        var (mockRequest, _, _) = await SendTransactionRequestAndValidateResponseAsync(request);

        Assert.Equal(TxnType.PreAuthComplete, request.TxnType);
        ValidatePreAuthManipulationRequest(request, mockRequest);
    }

    [Theory]
    [ClassData(typeof(PreAuthCompletionRequestNegativeTestDataBuilder))]
    public void PreAuthCompletionRequest_RequestContainsValidationError_ThrowsArgumentException(PreAuthCompletionRequest request,
        string invalidParam)
    {
        Service.SetPairSecret("123");

        var ex = Assert.Throws<ArgumentException>(() => Service.TransactionRequest(request));

        Assert.Contains(invalidParam, ex.Message);
    }

    [Fact]
    public async Task PreAuthExtendRequest_RequestValid_VerifyRequestAndResponse()
    {
        await PairAsync();
        var request = RequestRepository.GetPreAuthExtendRequest();

        var (mockRequest, _, _) = await SendTransactionRequestAndValidateResponseAsync(request);

        Assert.Equal(TxnType.PreAuthExtend, request.TxnType);
        ValidateTransactionRequest(request, mockRequest);
    }

    [Theory]
    [ClassData(typeof(PreAuthExtendRequestNegativeTestDataBuilder))]
    public void PreAuthExtendRequest_RequestContainsValidationError_ThrowsArgumentException(PreAuthExtendRequest request,
        string invalidParam)
    {
        Service.SetPairSecret("123");

        var ex = Assert.Throws<ArgumentException>(() => Service.TransactionRequest(request));

        Assert.Contains(invalidParam, ex.Message);
    }

    [Fact]
    public async Task PreAuthTopUpRequest_RequestValid_VerifyRequestAndResponse()
    {
        await PairAsync();
        var request = RequestRepository.GetPreAuthTopUpRequest();

        var (mockRequest, _, _) = await SendTransactionRequestAndValidateResponseAsync(request);

        Assert.Equal(TxnType.PreAuthTopUp, request.TxnType);
        ValidatePreAuthManipulationRequest(request, mockRequest);
    }

    [Theory]
    [ClassData(typeof(PreAuthTopUpNegativeTestDataBuilder))]
    public void PreAuthTopUpRequest_RequestContainsValidationError_ThrowsArgumentException(PreAuthTopUpRequest request,
        string invalidParam)
    {
        Service.SetPairSecret("123");

        var ex = Assert.Throws<ArgumentException>(() => Service.TransactionRequest(request));

        Assert.Contains(invalidParam, ex.Message);
    }

    [Fact]
    public async Task PreAuthPartialCancelRequest_RequestValid_VerifyRequestAndResponse()
    {
        await PairAsync();
        var request = RequestRepository.GetPreAuthPartialCancelRequest();

        var (mockRequest, _, _) = await SendTransactionRequestAndValidateResponseAsync(request);

        Assert.Equal(TxnType.PreAuthPartialCancel, request.TxnType);
        ValidatePreAuthManipulationRequest(request, mockRequest);
    }

    [Theory]
    [ClassData(typeof(PreAuthPartialCancelRequestNegativeTestDataBuilder))]
    public void PreAuthPartialCancelRequest_RequestContainsValidationError_ThrowsArgumentException(PreAuthPartialCancelRequest request,
        string invalidParam)
    {
        Service.SetPairSecret("123");

        var ex = Assert.Throws<ArgumentException>(() => Service.TransactionRequest(request));

        Assert.Contains(invalidParam, ex.Message);
    }

    [Fact]
    public async Task PreAuthSummaryRequest_RequestValid_VerifyRequestAndResponse()
    {
        await PairAsync();
        var request = RequestRepository.GetPreAuthSummaryRequest();

        await SendTransactionRequestAndValidateResponseAsync(request);

        Assert.Equal(TxnType.PreAuthInquiry, request.TxnType);
    }

    [Theory]
    [ClassData(typeof(PreAuthSummaryRequestNegativeTestDataBuilder))]
    public void PreAuthSummaryRequest_RequestContainsValidationError_ThrowsArgumentException(PreAuthSummaryRequest request,
        string invalidParam)
    {
        Service.SetPairSecret("123");

        var ex = Assert.Throws<ArgumentException>(() => Service.TransactionRequest(request));

        Assert.Contains(invalidParam, ex.Message);
    }

    [Fact]
    public async Task PreAuthInquiryRequest_RequestValid_VerifyRequestAndResponse()
    {
        await PairAsync();
        var request = RequestRepository.GetPreAuthInquiryRequest();

        await SendTransactionRequestAndValidateResponseAsync(request);

        Assert.Equal(TxnType.PreAuthInquiry, request.TxnType);
    }

    [Theory]
    [ClassData(typeof(PreAuthInquiryRequestNegativeTestDataBuilder))]
    public void PreAuthInquiryRequest_RequestContainsValidationError_ThrowsArgumentException(PreAuthInquiryRequest request,
        string invalidParam)
    {
        Service.SetPairSecret("123");

        var ex = Assert.Throws<ArgumentException>(() => Service.TransactionRequest(request));

        Assert.Contains(invalidParam, ex.Message);
    }

    [Fact]
    public async Task LogonRequest_RequestValid_VerifyRequestAndResponse()
    {
        await PairAsync();
        var request = RequestRepository.GetLogonRequest();
        LogonResponse? listenerResponse = null;

        MockListener
            .Setup(listener => listener.LogonComplete(It.IsAny<Guid>(), request, It.IsAny<LogonResponse>()))
            .Callback<Guid, LogonRequest, LogonResponse>((_, _, resp) => listenerResponse = resp);

        var sessionId = Service.LogonRequest(request);
        await Assertions.WaitForConditionAsync(() => listenerResponse is not null, Utils.CreateCancellationToken(),
            $"{nameof(IPosApiEventListener.LogonComplete)}() listener not invoked");

        Assert.NotNull(listenerResponse);
        Assert.True(MockMessageHandler.Session.Requests.TryGetSession(sessionId, out var mockBaseRequests, out var mockBaseResponses));
        var mockRequest = Assertions.SingleOfType<MockApi.Models.LogonRequest>(mockBaseRequests);
        var mockResponse = Assertions.SingleOfType<MockApi.Models.LogonResponse>(mockBaseResponses);

        ValidateLogonRequest(request, mockRequest);
        ValidateLogonResponse(mockResponse, listenerResponse);

        Assert.Equal(1, MockMessageHandler.Session.NumTokenRequests);
        MockListener.Verify(listener => listener.Display(sessionId, It.IsAny<PosApiRequest>(), It.IsAny<DisplayResponse>()), Times.Once);
        MockListener.Verify(listener => listener.Receipt(sessionId, It.IsAny<PosApiRequest>(), It.IsAny<ReceiptResponse>()), Times.AtLeastOnce);
        MockListener.Verify(listener => listener.LogonComplete(sessionId, It.IsAny<LogonRequest>(), It.IsAny<LogonResponse>()), Times.AtLeastOnce);
        MockListener.VerifyNoOtherCalls();
    }

    [Theory]
    [ClassData(typeof(LogonRequestNegativeTestDataBuilder))]
    public void LogonRequest_RequestContainsValidationError_ThrowsArgumentException(LogonRequest request, string invalidParam)
    {
        Service.SetPairSecret("123");

        var ex = Assert.Throws<ArgumentException>(() => Service.LogonRequest(request));

        Assert.Contains(invalidParam, ex.Message);
    }

    [Fact]
    public async Task SettlementRequest_RequestValid_VerifyRequestAndResponse()
    {
        await PairAsync();
        var request = RequestRepository.GetSettlementRequest();
        SettlementResponse? listenerResponse = null;

        MockListener
            .Setup(listener => listener.SettlementComplete(It.IsAny<Guid>(), request, It.IsAny<SettlementResponse>()))
            .Callback<Guid, SettlementRequest, SettlementResponse>((_, _, resp) => listenerResponse = resp);

        var sessionId = Service.SettlementRequest(request);
        await Assertions.WaitForConditionAsync(() => listenerResponse is not null, Utils.CreateCancellationToken(),
            $"{nameof(IPosApiEventListener.SettlementComplete)}() listener not invoked");

        Assert.NotNull(listenerResponse);
        Assert.True(MockMessageHandler.Session.Requests.TryGetSession(sessionId, out var mockBaseRequests, out var mockBaseResponses));
        var mockRequest = Assertions.SingleOfType<MockApi.Models.SettlementRequest>(mockBaseRequests);
        var mockResponse = Assertions.SingleOfType<MockApi.Models.SettlementResponse>(mockBaseResponses);

        ValidateSettlementRequest(request, mockRequest);
        ValidateSettlementResponse(mockResponse, listenerResponse);

        Assert.Equal(1, MockMessageHandler.Session.NumTokenRequests);
        MockListener.Verify(listener => listener.Display(sessionId, It.IsAny<PosApiRequest>(), It.IsAny<DisplayResponse>()), Times.AtLeastOnce);
        MockListener.Verify(listener => listener.Receipt(sessionId, It.IsAny<PosApiRequest>(), It.IsAny<ReceiptResponse>()), Times.AtLeastOnce);
        MockListener.Verify(listener => listener.SettlementComplete(sessionId, It.IsAny<SettlementRequest>(), It.IsAny<SettlementResponse>()),
            Times.Once);
        MockListener.VerifyNoOtherCalls();
    }

    [Theory]
    [ClassData(typeof(SettlementRequestNegativeTestDataBuilder))]
    public void SettlementRequest_RequestContainsValidationError_ThrowsArgumentException(SettlementRequest request, string invalidParam)
    {
        Service.SetPairSecret("123");

        var ex = Assert.Throws<ArgumentException>(() => Service.SettlementRequest(request));

        Assert.Contains(invalidParam, ex.Message);
    }

    [Fact]
    public async Task StatusRequest_RequestValid_VerifyRequestAndResponse()
    {
        await PairAsync();
        var request = RequestRepository.GetStatusRequest();
        StatusResponse? listenerResponse = null;

        MockListener
            .Setup(listener => listener.StatusComplete(It.IsAny<Guid>(), request, It.IsAny<StatusResponse>()))
            .Callback<Guid, StatusRequest, StatusResponse>((_, _, resp) => listenerResponse = resp);

        var sessionId = Service.StatusRequest(request);
        await Assertions.WaitForConditionAsync(() => listenerResponse is not null, Utils.CreateCancellationToken(),
            $"{nameof(IPosApiEventListener.StatusComplete)}() listener not invoked");

        Assert.NotNull(listenerResponse);
        Assert.True(MockMessageHandler.Session.Requests.TryGetSession(sessionId, out var mockBaseRequests, out var mockBaseResponses));
        var mockRequest = Assertions.SingleOfType<MockApi.Models.StatusRequest>(mockBaseRequests);
        var mockResponse = Assertions.SingleOfType<MockApi.Models.StatusResponse>(mockBaseResponses);

        ValidateStatusRequest(request, mockRequest);
        ValidateStatusResponse(mockResponse, listenerResponse);

        Assert.Equal(1, MockMessageHandler.Session.NumTokenRequests);
        MockListener.Verify(listener => listener.StatusComplete(sessionId, It.IsAny<StatusRequest>(), It.IsAny<StatusResponse>()),
            Times.Once);
        MockListener.VerifyNoOtherCalls();
    }

    [Theory]
    [ClassData(typeof(StatusRequestNegativeTestDataBuilder))]
    public void StatusRequest_RequestContainsValidationError_ThrowsArgumentException(StatusRequest request, string invalidParam)
    {
        Service.SetPairSecret("123");

        var ex = Assert.Throws<ArgumentException>(() => Service.StatusRequest(request));

        Assert.Contains(invalidParam, ex.Message);
    }

    [Fact]
    public async Task QueryCardRequest_RequestValid_VerifyRequestAndResponse()
    {
        await PairAsync();
        var request = RequestRepository.GetQueryCardRequest();
        QueryCardResponse? listenerResponse = null;

        MockListener
            .Setup(listener => listener.QueryCardComplete(It.IsAny<Guid>(), request, It.IsAny<QueryCardResponse>()))
            .Callback<Guid, QueryCardRequest, QueryCardResponse>((_, _, resp) => listenerResponse = resp);

        var sessionId = Service.QueryCardRequest(request);
        await Assertions.WaitForConditionAsync(() => listenerResponse is not null, Utils.CreateCancellationToken(),
            $"{nameof(IPosApiEventListener.QueryCardComplete)}() listener not invoked");

        Assert.NotNull(listenerResponse);
        Assert.True(MockMessageHandler.Session.Requests.TryGetSession(sessionId, out var mockBaseRequests, out var mockBaseResponses));
        var mockRequest = Assertions.SingleOfType<MockApi.Models.QueryCardRequest>(mockBaseRequests);
        var mockResponse = Assertions.SingleOfType<MockApi.Models.QueryCardResponse>(mockBaseResponses);

        ValidateQueryCardRequest(request, mockRequest);
        ValidateQueryCardResponse(mockResponse, listenerResponse);

        Assert.Equal(1, MockMessageHandler.Session.NumTokenRequests);
        MockListener.Verify(listener => listener.Display(sessionId, It.IsAny<PosApiRequest>(), It.IsAny<DisplayResponse>()), Times.AtLeastOnce);
        MockListener.Verify(listener => listener.QueryCardComplete(sessionId, It.IsAny<QueryCardRequest>(), It.IsAny<QueryCardResponse>()),
            Times.Once);
        MockListener.VerifyNoOtherCalls();
    }

    [Theory]
    [ClassData(typeof(QueryCardRequestNegativeTestDataBuilder))]
    public void QueryCardRequest_RequestContainsValidationError_ThrowsArgumentException(QueryCardRequest request, string invalidParam)
    {
        Service.SetPairSecret("123");

        var ex = Assert.Throws<ArgumentException>(() => Service.QueryCardRequest(request));

        Assert.Contains(invalidParam, ex.Message);
    }

    [Fact]
    public async Task ConfigureMerchantRequest_RequestValid_VerifyRequestAndResponse()
    {
        await PairAsync();
        var request = RequestRepository.GetConfigureMerchantRequest();
        ConfigureMerchantResponse? listenerResponse = null;

        MockListener
            .Setup(listener => listener.ConfigureMerchantComplete(It.IsAny<Guid>(), request, It.IsAny<ConfigureMerchantResponse>()))
            .Callback<Guid, ConfigureMerchantRequest, ConfigureMerchantResponse>((_, _, resp) => listenerResponse = resp);

        var sessionId = Service.ConfigureMerchantRequest(request);
        await Assertions.WaitForConditionAsync(() => listenerResponse is not null, Utils.CreateCancellationToken(),
            $"{nameof(IPosApiEventListener.ConfigureMerchantComplete)}() listener not invoked");

        Assert.NotNull(listenerResponse);
        Assert.True(MockMessageHandler.Session.Requests.TryGetSession(sessionId, out var mockBaseRequests, out var mockBaseResponses));
        var mockRequest = Assertions.SingleOfType<MockApi.Models.ConfigureMerchantRequest>(mockBaseRequests);
        var mockResponse = Assertions.SingleOfType<MockApi.Models.ConfigureMerchantResponse>(mockBaseResponses);

        ValidateConfigureMerchantRequest(request, mockRequest);
        ValidateConfigureMerchantResponse(mockResponse, listenerResponse);

        Assert.Equal(1, MockMessageHandler.Session.NumTokenRequests);
        MockListener.Verify(
            listener => listener.ConfigureMerchantComplete(sessionId, It.IsAny<ConfigureMerchantRequest>(), It.IsAny<ConfigureMerchantResponse>()),
            Times.Once);
        MockListener.VerifyNoOtherCalls();
    }

    [Theory]
    [ClassData(typeof(ConfigureMerchantRequestNegativeTestDataBuilder))]
    public void ConfigureMerchantRequest_RequestContainsValidationError_ThrowsArgumentException(ConfigureMerchantRequest request,
        string invalidParam)
    {
        Service.SetPairSecret("123");

        var ex = Assert.Throws<ArgumentException>(() => Service.ConfigureMerchantRequest(request));

        Assert.Contains(invalidParam, ex.Message);
    }

    [Fact]
    public async Task ReprintReceiptRequest_RequestValid_VerifyRequestAndResponse()
    {
        await PairAsync();
        var request = RequestRepository.GetReprintReceiptRequest();
        ReprintReceiptResponse? listenerResponse = null;

        MockListener
            .Setup(listener => listener.ReprintReceiptComplete(It.IsAny<Guid>(), request, It.IsAny<ReprintReceiptResponse>()))
            .Callback<Guid, ReprintReceiptRequest, ReprintReceiptResponse>((_, _, resp) => listenerResponse = resp);

        var sessionId = Service.ReprintReceiptRequest(request);
        await Assertions.WaitForConditionAsync(() => listenerResponse is not null, Utils.CreateCancellationToken(),
            $"{nameof(IPosApiEventListener.ReprintReceiptComplete)}() listener not invoked");

        Assert.NotNull(listenerResponse);
        Assert.True(MockMessageHandler.Session.Requests.TryGetSession(sessionId, out var mockBaseRequests, out var mockBaseResponses));
        var mockRequest = Assertions.SingleOfType<MockApi.Models.ReprintReceiptRequest>(mockBaseRequests);
        var mockResponse = Assertions.SingleOfType<MockApi.Models.ReprintReceiptResponse>(mockBaseResponses);

        ValidateReprintReceiptRequest(request, mockRequest);
        ValidateReprintReceiptResponse(mockResponse, listenerResponse);

        Assert.Equal(1, MockMessageHandler.Session.NumTokenRequests);
        MockListener.Verify(
            listener => listener.ReprintReceiptComplete(sessionId, It.IsAny<ReprintReceiptRequest>(), It.IsAny<ReprintReceiptResponse>()),
            Times.Once);
        MockListener.VerifyNoOtherCalls();
    }

    [Theory]
    [ClassData(typeof(ReprintReceiptRequestNegativeTestDataBuilder))]
    public void ReprintReceiptRequest_RequestContainsValidationError_ThrowsArgumentException(ReprintReceiptRequest request,
        string invalidParam)
    {
        Service.SetPairSecret("123");

        var ex = Assert.Throws<ArgumentException>(() => Service.ReprintReceiptRequest(request));

        Assert.Contains(invalidParam, ex.Message);
    }

    [Fact]
    public async Task SendKeyRequest_RequestValid_VerifyRequestAndResponse()
    {
        await PairAsync();
        var (_, _, sessionId) = await SendTransactionRequestAndValidateResponseAsync(RequestRepository.GetPurchaseRequest());
        var request = RequestRepository.GetSendKeyRequest(sessionId);
        MockApi.Models.SendKeyRequest? mockRequest = null;

        Service.SendKeyRequest(request);
        await Assertions.WaitForConditionAsync(() =>
            {
                // check if the send key request was received by the mock API
                Assert.True(MockMessageHandler.Session.Requests.TryGetSession(sessionId, out var mockBaseRequests, out _));
                mockRequest = mockBaseRequests.Select(req => req).OfType<MockApi.Models.SendKeyRequest>().SingleOrDefault();
                return mockRequest is not null;
            }, Utils.CreateCancellationToken(),
            $"{nameof(MockApi.Models.SendKeyRequest)} not received.");

        Assert.NotNull(mockRequest);
        ValidateSendKeyRequest(request, mockRequest);
        Assert.Equal(1, MockMessageHandler.Session.NumTokenRequests);
        MockListener.VerifyNoOtherCalls();
    }

    [Theory]
    [ClassData(typeof(SendKeyRequestNegativeTestDataBuilder))]
    public void SendKeyRequest_RequestContainsValidationError_ThrowsArgumentException(SendKeyRequest request, string invalidParam)
    {
        Service.SetPairSecret("123");

        var ex = Assert.Throws<ArgumentException>(() => Service.SendKeyRequest(request));

        Assert.Contains(invalidParam, ex.Message);
    }

    [Fact]
    public async Task ResultRequest_RequestValid_VerifyRequestAndResponse()
    {
        // set up
        await PairAsync();
        var (_, _, sessionId) = await SendTransactionRequestAndValidateResponseAsync(RequestRepository.GetPurchaseRequest());
        var request = RequestRepository.GetResultRequest(sessionId);
        IEnumerable<PosApiResponse>? listenerResponse = null;

        MockListener
            .Setup(listener => listener.ResultComplete(request, It.IsAny<ICollection<PosApiResponse>>()))
            .Callback<ResultRequest, IEnumerable<PosApiResponse>>((_, resp) => listenerResponse = resp);

        // execute
        Service.ResultRequest(request);
        await Assertions.WaitForConditionAsync(() => listenerResponse is not null, Utils.CreateCancellationToken(),
            $"{nameof(IPosApiEventListener.ResultComplete)}() listener not invoked");

        // verify
        Assert.NotNull(listenerResponse);
        Assert.True(MockMessageHandler.Session.Requests.TryGetSession(sessionId, out _, out var mockBaseResponses));

        foreach (var (_, posApiResponse, mockBaseResponse) in Utils.Zip(listenerResponse, mockBaseResponses))
            switch (posApiResponse)
            {
                case DisplayResponse displayResponse:
                {
                    var mockDisplayResponse = Assert.IsType<MockApi.Models.DisplayResponse>(mockBaseResponse);
                    ValidateDisplayResponse(mockDisplayResponse, displayResponse);
                    break;
                }
                case ReceiptResponse receiptResponse:
                {
                    var mockReceiptResponse = Assert.IsType<MockApi.Models.ReceiptResponse>(mockBaseResponse);
                    ValidateReceiptResponse(mockReceiptResponse, receiptResponse);
                    break;
                }
                case TransactionResponse transactionResponse:
                {
                    var mockTransactionResponse = Assert.IsType<MockApi.Models.TransactionResponse>(mockBaseResponse);
                    ValidateTransactionResponse(mockTransactionResponse, transactionResponse);
                    break;
                }
                default:
                    Assert.Fail($"Unexpected response type: {posApiResponse.ResponseType}");
                    break;
            }

        Assert.Equal(1, MockMessageHandler.Session.NumTokenRequests);
        MockListener.Verify(
            listener => listener.ResultComplete(It.IsAny<ResultRequest>(), It.IsAny<ICollection<PosApiResponse>>()),
            Times.Once);
        MockListener.VerifyNoOtherCalls();
    }

    [Theory]
    [ClassData(typeof(ResultRequestNegativeTestDataBuilder))]
    public void ResultRequest_RequestContainsValidationError_ThrowsArgumentException(ResultRequest request, string invalidParam)
    {
        Service.SetPairSecret("123");

        var ex = Assert.Throws<ArgumentException>(() => Service.ResultRequest(request));

        Assert.Contains(invalidParam, ex.Message);
    }

    [Theory]
    [InlineData(ReferenceType.RRN)]
    [InlineData(ReferenceType.ReferenceNo)]
    public async Task RetrieveTransactionRequest_RequestValid_VerifyRequestAndResponse(ReferenceType referenceType)
    {
        await PairAsync();
        var purchaseRequest = RequestRepository.GetPurchaseRequest();
        var (_, mockResponse, _) = await SendTransactionRequestAndValidateResponseAsync(purchaseRequest);
        TransactionResponse? listenerResponse = null;
        var retrieveTransactionRequest = new RetrieveTransactionRequest
        {
            ReferenceType = referenceType,
            Reference = (referenceType == ReferenceType.RRN ? purchaseRequest.RRN : purchaseRequest.TxnRef)!
        };
        
        MockListener
            .Setup(listener => listener.RetrieveTransactionComplete(retrieveTransactionRequest, It.IsAny<ICollection<TransactionResponse>>()))
            .Callback<RetrieveTransactionRequest, ICollection<TransactionResponse>>((_, resp) => listenerResponse = resp.Single());

        Service.RetrieveTransactionRequest(retrieveTransactionRequest);
        await Assertions.WaitForConditionAsync(() => listenerResponse is not null, Utils.CreateCancellationToken(),
            $"{nameof(IPosApiEventListener.RetrieveTransactionComplete)}() listener not invoked");

        Assert.NotNull(listenerResponse);
        ValidateTransactionResponse(mockResponse, listenerResponse);
        MockListener.Verify(listener => listener.RetrieveTransactionComplete(It.IsAny<RetrieveTransactionRequest>(), It.IsAny<ICollection<TransactionResponse>>()), Times.Once);
        MockListener.VerifyNoOtherCalls();
    }


    [Fact]
    public async Task RetrieveTransactionRequest_ReferenceNumberDoesNotExist_ErrorListenerInvoked()
    {
        await PairAsync();
        
        var retrieveTransactionRequest = new RetrieveTransactionRequest
        {
            ReferenceType = ReferenceType.ReferenceNo,
            Reference = "REF_DOES_NOT_EXIST"
        };
        var expectedError = new HttpResponseMessage(HttpStatusCode.NotFound);

        await SendRequestAndValidateErrorResponseAsync(Service.RetrieveTransactionRequest, retrieveTransactionRequest, expectedError);
    }

    [Theory]
    [ClassData(typeof(RetrieveTransactionRequestNegativeTestDataBuilder))]
    public void RetrieveTransactionRequest_RequestContainsValidationError_ThrowsArgumentException(RetrieveTransactionRequest request, string invalidParam)
    {
        Service.SetPairSecret("123");

        var ex = Assert.Throws<ArgumentException>(() => Service.RetrieveTransactionRequest(request));

        Assert.Contains(invalidParam, ex.Message);
    }

    [Fact]
    public void PosApiRequest_ServiceNotPaired_ThrowsInvalidOperationException()
    {
        Assert.Throws<InvalidOperationException>(() => Service.TransactionRequest(RequestRepository.GetPurchaseRequest()));
        Assert.Throws<InvalidOperationException>(() => Service.StatusRequest(RequestRepository.GetStatusRequest()));
        Assert.Throws<InvalidOperationException>(() => Service.LogonRequest(RequestRepository.GetLogonRequest()));
        Assert.Throws<InvalidOperationException>(() => Service.SettlementRequest(RequestRepository.GetSettlementRequest()));
        Assert.Throws<InvalidOperationException>(() => Service.QueryCardRequest(RequestRepository.GetQueryCardRequest()));
        Assert.Throws<InvalidOperationException>(() => Service.ConfigureMerchantRequest(RequestRepository.GetConfigureMerchantRequest()));
        Assert.Throws<InvalidOperationException>(() => Service.ReprintReceiptRequest(RequestRepository.GetReprintReceiptRequest()));
        Assert.Throws<InvalidOperationException>(() => Service.SendKeyRequest(RequestRepository.GetSendKeyRequest(Guid.NewGuid())));
        Assert.Throws<InvalidOperationException>(() => Service.ResultRequest(RequestRepository.GetResultRequest(Guid.NewGuid())));
    }

    [Fact]
    public async Task PosApiRequest_HttpRequestExceptionThrownSendingAuthRequest_ErrorListenerInvoked()
    {
        await PairAsync();

        var requestException = MockMessageHandler.Options.AuthRequestException = new HttpRequestException();

        await SendRequestAndValidateErrorResponseAsync(Service.TransactionRequest, RequestRepository.GetPurchaseRequest(),
            requestException);
    }

    [Fact]
    public async Task PosApiRequest_UnauthorizedErrorResponseSendingAuthRequest_ErrorListenerInvoked()
    {
        await PairAsync();

        var requestError = MockMessageHandler.Options.AuthRequestError = new HttpResponseMessage(HttpStatusCode.Unauthorized)
            { Content = new StringContent("Unauthorised") };

        await SendRequestAndValidateErrorResponseAsync(Service.TransactionRequest, RequestRepository.GetPurchaseRequest(), requestError);
    }

    [Fact]
    public async Task PosApiRequest_HttpRequestExceptionThrownSendingRequest_ErrorListenerInvoked()
    {
        await PairAsync();

        var requestException = MockMessageHandler.Options.PosRequestException = new HttpRequestException();

        await SendRequestAndValidateErrorResponseAsync(Service.TransactionRequest, RequestRepository.GetPurchaseRequest(),
            requestException);
    }

    [Fact]
    public async Task PosApiRequest_UnauthorizedErrorResponseSendingRequest_ErrorListenerInvoked()
    {
        await PairAsync();

        var requestError = MockMessageHandler.Options.PosRequestError = new HttpResponseMessage(HttpStatusCode.Unauthorized)
            { Content = new StringContent("Unauthorised") };

        await SendRequestAndValidateErrorResponseAsync(Service.TransactionRequest, RequestRepository.GetPurchaseRequest(), requestError);
    }

    [Fact]
    public async Task PosApiRequest_HttpRequestExceptionThrownSendingResultRequest_ErrorListenerInvoked()
    {
        await PairAsync();

        var requestException = MockMessageHandler.Options.ResultRequestException = new HttpRequestException();

        await SendRequestAndValidateErrorResponseAsync(Service.TransactionRequest, RequestRepository.GetPurchaseRequest(),
            requestException);
    }

    [Fact]
    public async Task PosApiRequest_UnauthorizedErrorResponseSendingResultRequest_ErrorListenerInvoked()
    {
        await PairAsync();

        var requestError = MockMessageHandler.Options.ResultRequestError = new HttpResponseMessage(HttpStatusCode.Unauthorized)
            { Content = new StringContent("Unauthorised") };

        await SendRequestAndValidateErrorResponseAsync(Service.TransactionRequest, RequestRepository.GetPurchaseRequest(), requestError);
    }

    [Fact]
    public async Task PosApiRequest_AsyncRequestTimeoutExceeded_ErrorListenerInvoked()
    {
        await PairAsync();
        ServiceOptions.AsyncRequestTimeout = TimeSpan.Zero;
        var request = RequestRepository.GetPurchaseRequest();

        var sessionId = Service.TransactionRequest(request);
        await Task.Delay(500);

        MockListener.Verify(listener =>
            listener.Error(sessionId, request, It.Is<ErrorResponse>(resp => resp.Source == ErrorSource.Internal)), Times.Once);
    }
}