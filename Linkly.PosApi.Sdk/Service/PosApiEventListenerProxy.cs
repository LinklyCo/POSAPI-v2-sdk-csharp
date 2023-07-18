using System;
using System.Collections.Generic;
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
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Linkly.PosApi.Sdk.Service
{
    /// <summary>
    /// Event listener proxy for <see cref="IPosApiEventListener" /> which will forward requests to a real
    /// event listener and provide error handling and logging.
    /// </summary>
    internal class PosApiEventListenerProxy : IPosApiEventListener
    {
        private readonly WeakReference<IPosApiEventListener> _eventListener;
        private readonly ILogger _logger;

        public PosApiEventListenerProxy(ILogger? logger, IPosApiEventListener eventListener)
        {
            _logger = logger ?? NullLogger.Instance;
            _eventListener = new WeakReference<IPosApiEventListener>(null!);
            SetEventListener(eventListener);
        }

        private IPosApiEventListener? EventListener
        {
            get
            {
                if (_eventListener.TryGetTarget(out var listener))
                    return listener;

                _logger.LogError("{Method}: Event listener does not exist", nameof(InvokeListener));
                return null;
            }
        }

        public void Receipt(Guid sessionId, PosApiRequest request, ReceiptResponse response) =>
            InvokeListener(() => EventListener?.Receipt(sessionId, request, response), nameof(Receipt));

        public void Display(Guid sessionId, PosApiRequest request, DisplayResponse response) =>
            InvokeListener(() => EventListener?.Display(sessionId, request, response), nameof(Display));

        public void Error(Guid? sessionId, IBaseRequest request, ErrorResponse error) =>
            InvokeListener(() => EventListener?.Error(sessionId, request, error), nameof(Error));

        public void PairingComplete(PairingRequest request, PairingResponse response) =>
            InvokeListener(() => EventListener?.PairingComplete(request, response), nameof(PairingComplete));

        public void TransactionComplete(Guid sessionId, TransactionRequest request, TransactionResponse response) =>
            InvokeListener(() => EventListener?.TransactionComplete(sessionId, request, response), nameof(TransactionComplete));

        public void StatusComplete(Guid sessionId, StatusRequest request, StatusResponse response) =>
            InvokeListener(() => EventListener?.StatusComplete(sessionId, request, response), nameof(StatusComplete));

        public void LogonComplete(Guid sessionId, LogonRequest request, LogonResponse response) =>
            InvokeListener(() => EventListener?.LogonComplete(sessionId, request, response), nameof(LogonComplete));

        public void SettlementComplete(Guid sessionId, SettlementRequest request, SettlementResponse response) =>
            InvokeListener(() => EventListener?.SettlementComplete(sessionId, request, response), nameof(SettlementComplete));

        public void QueryCardComplete(Guid sessionId, QueryCardRequest request, QueryCardResponse response) =>
            InvokeListener(() => EventListener?.QueryCardComplete(sessionId, request, response), nameof(QueryCardComplete));

        public void ConfigureMerchantComplete(Guid sessionId, ConfigureMerchantRequest request, ConfigureMerchantResponse response) =>
            InvokeListener(() => EventListener?.ConfigureMerchantComplete(sessionId, request, response), nameof(ConfigureMerchantComplete));

        public void ReprintReceiptComplete(Guid sessionId, ReprintReceiptRequest request, ReprintReceiptResponse response) =>
            InvokeListener(() => EventListener?.ReprintReceiptComplete(sessionId, request, response), nameof(ReprintReceiptComplete));

        public void ResultComplete(ResultRequest request, ICollection<PosApiResponse> responses) =>
            InvokeListener(() => EventListener?.ResultComplete(request, responses), nameof(ResultComplete));

        public void RetrieveTransactionComplete(RetrieveTransactionRequest request, ICollection<TransactionResponse> responses) =>
            InvokeListener(() => EventListener?.RetrieveTransactionComplete(request, responses), nameof(RetrieveTransactionComplete));

        /// <summary>Set the underlying event listener implementation the proxy will invoke.</summary>
        /// <param name="eventListener">Event listener implementation.</param>
        /// <exception cref="ArgumentNullException"></exception>
        public void SetEventListener(IPosApiEventListener eventListener)
        {
            if (eventListener is null)
            {
                _logger.LogError("{Method}: {Param} is null", nameof(SetEventListener), nameof(eventListener));
                throw new ArgumentNullException(nameof(eventListener), "Required");
            }

            _eventListener.SetTarget(eventListener);
        }

        /// <summary>Invoke a listener with logging and exception handling.</summary>
        /// <param name="listener">Listener to invoke wrapped in a lambda</param>
        /// <param name="listenerName">Name of the listener, for logging purposes.</param>
        private void InvokeListener(Action listener, string listenerName)
        {
            try
            {
                _logger.LogTrace("{Method}: Invoking {Listener}() listener", nameof(InvokeListener), listenerName);
                listener();
                _logger.LogTrace("{Method}: {Listener}() listener completed", nameof(InvokeListener), listenerName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{Method}: {Listener}() listener threw an exception", nameof(InvokeListener), listenerName);
            }
        }
    }
}