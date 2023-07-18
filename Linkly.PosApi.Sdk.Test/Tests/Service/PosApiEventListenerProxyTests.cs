using System.Diagnostics.CodeAnalysis;
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
using Linkly.PosApi.Sdk.Models.Settlement;
using Linkly.PosApi.Sdk.Models.Status;
using Linkly.PosApi.Sdk.Models.Transaction;
using Linkly.PosApi.Sdk.Service;
using Linkly.PosApi.Sdk.UnitTest.Common;

namespace Linkly.PosApi.Sdk.UnitTest.Tests.Service;

public class PosApiEventListenerProxyTests
{
    private static readonly Guid SessionId = new("9c567474-a195-4e26-a09e-609354affabf");
    private readonly PosApiEventListenerProxy _eventProxy;
    private readonly StoreLogger _logger = new();
    private Mock<IPosApiEventListener> _mockListener = new();

    public PosApiEventListenerProxyTests() => _eventProxy = new PosApiEventListenerProxy(_logger, _mockListener.Object);

    [Fact]
    public void Initialise_EventListenerNull_ThrowsArgumentNullException()
    {
        var ex = Assert.Throws<ArgumentNullException>(() => new PosApiEventListenerProxy(null, null!));
        Assert.Equal("eventListener", ex.ParamName);
    }

    [Fact]
    public void SetEventListener_EventListenerIsNull_ThrowsArgumentNullException()
    {
        var ex = Assert.Throws<ArgumentNullException>(() => _eventProxy.SetEventListener(null!));
        Assert.Equal("eventListener", ex.ParamName);
    }

    [Fact]
    public void Receipt_ListenerInvoked()
    {
        var (request, response) = (new PurchaseRequest(), new ReceiptResponse());

        _eventProxy.Receipt(SessionId, request, response);

        _mockListener.Verify(listener => listener.Receipt(SessionId, request, response), Times.Once);
    }

    [Fact]
    public void Display_ListenerInvoked()
    {
        var (request, response) = (new PurchaseRequest(), new DisplayResponse());

        _eventProxy.Display(SessionId, request, response);

        _mockListener.Verify(listener => listener.Display(SessionId, request, response), Times.Once);
    }

    [Fact]
    public void Error_ListenerInvoked()
    {
        var (request, response) = (new PurchaseRequest(), new ErrorResponse(ErrorSource.Internal, null, HttpStatusCode.BadRequest));

        _eventProxy.Error(SessionId, request, response);

        _mockListener.Verify(listener => listener.Error(SessionId, request, response), Times.Once);
    }

    [Fact]
    public void PairingComplete_ListenerInvoked()
    {
        var (request, response) = (new PairingRequest(), new PairingResponse());

        _eventProxy.PairingComplete(request, response);

        _mockListener.Verify(listener => listener.PairingComplete(request, response), Times.Once);
    }

    [Fact]
    public void TransactionComplete_ListenerInvoked()
    {
        var (request, response) = (new PurchaseRequest(), new TransactionResponse());

        _eventProxy.TransactionComplete(SessionId, request, response);

        _mockListener.Verify(listener => listener.TransactionComplete(SessionId, request, response), Times.Once);
    }

    [Fact]
    public void StatusComplete_ListenerInvoked()
    {
        var (request, response) = (new StatusRequest(), new StatusResponse());

        _eventProxy.StatusComplete(SessionId, request, response);

        _mockListener.Verify(listener => listener.StatusComplete(SessionId, request, response), Times.Once);
    }

    [Fact]
    public void LogonComplete_ListenerInvoked()
    {
        var (request, response) = (new LogonRequest(), new LogonResponse());

        _eventProxy.LogonComplete(SessionId, request, response);

        _mockListener.Verify(listener => listener.LogonComplete(SessionId, request, response), Times.Once);
    }

    [Fact]
    public void SettlementComplete_ListenerInvoked()
    {
        var (request, response) = (new SettlementRequest(), new SettlementResponse());

        _eventProxy.SettlementComplete(SessionId, request, response);

        _mockListener.Verify(listener => listener.SettlementComplete(SessionId, request, response), Times.Once);
    }

    [Fact]
    public void QueryCardComplete_ListenerInvoked()
    {
        var (request, response) = (new QueryCardRequest(), new QueryCardResponse());

        _eventProxy.QueryCardComplete(SessionId, request, response);

        _mockListener.Verify(listener => listener.QueryCardComplete(SessionId, request, response), Times.Once);
    }

    [Fact]
    public void ConfigureMerchantComplete_ListenerInvoked()
    {
        var (request, response) = (new ConfigureMerchantRequest(), new ConfigureMerchantResponse());

        _eventProxy.ConfigureMerchantComplete(SessionId, request, response);

        _mockListener.Verify(listener => listener.ConfigureMerchantComplete(SessionId, request, response), Times.Once);
    }

    [Fact]
    public void ReprintReceiptComplete_ListenerInvoked()
    {
        var (request, response) = (new ReprintReceiptRequest(), new ReprintReceiptResponse());

        _eventProxy.ReprintReceiptComplete(SessionId, request, response);

        _mockListener.Verify(listener => listener.ReprintReceiptComplete(SessionId, request, response), Times.Once);
    }

    [Fact]
    public void ResultComplete_ListenerInvoked()
    {
        var (request, response) = (new ResultRequest(Guid.Empty), new List<PosApiResponse>());

        _eventProxy.ResultComplete(request, response);

        _mockListener.Verify(listener => listener.ResultComplete(request, response), Times.Once);
    }

    [Fact]
    public void RetrieveTransactionComplete_ListenerInvoked()
    {
        var (request, response) = (new RetrieveTransactionRequest(), new List<TransactionResponse>());

        _eventProxy.RetrieveTransactionComplete(request, response);

        _mockListener.Verify(listener => listener.RetrieveTransactionComplete(request, response), Times.Once);
    }

    [Fact]
    public void SetEventListener_SwitchEventListener_PairingRequestInvokedOnlyOnNewListener()
    {
        var mockListener = new Mock<IPosApiEventListener>();
        _eventProxy.SetEventListener(mockListener.Object);

        _eventProxy.PairingComplete(new PairingRequest(), new PairingResponse());

        mockListener.Verify(listener => listener.PairingComplete(It.IsAny<PairingRequest>(), It.IsAny<PairingResponse>()), Times.Once);
        _mockListener.Verify(listener => listener.PairingComplete(It.IsAny<PairingRequest>(), It.IsAny<PairingResponse>()), Times.Never);
    }

    [Fact]
    public void InvokeListener_ListenerThrowsException_ExceptionLogged()
    {
        _mockListener
            .Setup(listener => listener.PairingComplete(It.IsAny<PairingRequest>(), It.IsAny<PairingResponse>()))
            .Throws<InvalidOperationException>();

        _eventProxy.PairingComplete(new PairingRequest(), new PairingResponse());

        Assert.Contains($"{nameof(_eventProxy.PairingComplete)}() listener threw an exception", _logger.Logs.Last().Message);
    }

    [Fact]
    [SuppressMessage("SonarLint", "S1215", Justification = "GC.Collect() triggered intentionally for unit testing purposes")]
    public void InvokeListener_ListenerRemovedFromMemory_ExceptionLogged()
    {
        _mockListener = null!;
        GC.Collect();

        _eventProxy.PairingComplete(new PairingRequest(), new PairingResponse());

        Assert.Contains("InvokeListener: Event listener does not exist", _logger.Logs.Select(log => log.Message));
    }
}