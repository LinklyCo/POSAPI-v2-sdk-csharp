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

namespace Linkly.PosApi.Sdk.Service
{
    /// <summary>
    /// Interface for listening to POS API v2 responses. This interface needs to be implemented and
    /// all methods should provide logic for handling the response.
    /// </summary>
    public interface IPosApiEventListener
    {
        /// <summary>Receipt event handler. Will be invoked automatically during the course of
        /// some requests when a receipt event is fired by the PIN pad.</summary>
        /// <param name="sessionId">Session ID of the original request.</param>
        /// <param name="request">Original request the event belongs to.</param>
        /// <param name="response">Receipt message from the PIN pad.</param>
        void Receipt(Guid sessionId, PosApiRequest request, ReceiptResponse response);

        /// <summary>PIN pad display event handler. Will be invoked automatically during the course of
        /// some requests when the PIN pad displays some text to the customer.</summary>
        /// <param name="sessionId">Session ID of the original request.</param>
        /// <param name="request">Original request the event belongs to.</param>
        /// <param name="response">Display message from the PIN pad.</param>
        void Display(Guid sessionId, PosApiRequest request, DisplayResponse response);

        /// <summary>API error response handler, invoked when the API returns an unsuccessful response to
        /// a request.</summary>
        /// <param name="sessionId">Session ID of the original request. Will be null for requests which
        /// do not use sessions such as pairing.</param>
        /// <param name="request">Original request the event belongs to.</param>
        /// <param name="error">Error message thrown by the API.</param>
        void Error(Guid? sessionId, IBaseRequest request, ErrorResponse error);

        /// <summary>Pairing complete handler. Returns the secret which should be stored by the POS.</summary>
        /// <param name="request">Original request the event belongs to.</param>
        /// <param name="response">Pairing completion response</param>
        void PairingComplete(PairingRequest request, PairingResponse response);

        /// <summary>Transaction completion handler.</summary>
        /// <param name="sessionId">Session ID of the original request.</param>
        /// <param name="request">Original request the event belongs to.</param>
        /// <param name="response">Transaction completion response</param>
        void TransactionComplete(Guid sessionId, TransactionRequest request, TransactionResponse response);

        /// <summary>Status request completion handler.</summary>
        /// <param name="sessionId">Session ID of the original request.</param>
        /// <param name="request">Original request the event belongs to.</param>
        /// <param name="response">Status completion response</param>
        void StatusComplete(Guid sessionId, StatusRequest request, StatusResponse response);

        /// <summary>Logon completion handler.</summary>
        /// <param name="sessionId">Session ID of the original request.</param>
        /// <param name="request">Original request the event belongs to.</param>
        /// <param name="response">Logon completion response</param>
        void LogonComplete(Guid sessionId, LogonRequest request, LogonResponse response);

        /// <summary>Settlement completion handler.</summary>
        /// <param name="sessionId">Session ID of the original request.</param>
        /// <param name="request">Original request the event belongs to.</param>
        /// <param name="response">Settlement completion response</param>
        void SettlementComplete(Guid sessionId, SettlementRequest request, SettlementResponse response);

        /// <summary>Query card completion handler.</summary>
        /// <param name="sessionId">Session ID of the original request.</param>
        /// <param name="request">Original request the event belongs to.</param>
        /// <param name="response">Querycard completion response</param>
        void QueryCardComplete(Guid sessionId, QueryCardRequest request, QueryCardResponse response);

        /// <summary>Configure merchant completion handler.</summary>
        /// <param name="sessionId">Session ID of the original request.</param>
        /// <param name="request">Original request the event belongs to.</param>
        /// <param name="response">Configure merchant completion response</param>
        void ConfigureMerchantComplete(Guid sessionId, ConfigureMerchantRequest request, ConfigureMerchantResponse response);

        /// <summary>Reprint receipt completion handler.</summary>
        /// <param name="sessionId">Session ID of the original request.</param>
        /// <param name="request">Original request the event belongs to.</param>
        /// <param name="response">Reprint receipt completion response</param>
        void ReprintReceiptComplete(Guid sessionId, ReprintReceiptRequest request, ReprintReceiptResponse response);

        /// <summary>Result completion handler.</summary>
        /// <param name="request">Original request the event belongs to.</param>
        /// <param name="responses">
        /// All current responses associated with the
        /// <see cref="ResultRequest.SessionId" />.
        /// </param>
        void ResultComplete(ResultRequest request, ICollection<PosApiResponse> responses);

        /// <summary>Retrieve transaction completion handler.</summary>
        /// <param name="request">Original request the event belongs to.</param>
        /// <param name="responses">Transaction result(s)</param>
        void RetrieveTransactionComplete(RetrieveTransactionRequest request, ICollection<TransactionResponse> responses);
    }
}