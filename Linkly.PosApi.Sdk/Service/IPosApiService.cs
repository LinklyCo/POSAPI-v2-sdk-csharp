using System;
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

namespace Linkly.PosApi.Sdk.Service
{
    /// <summary>
    /// Interface for communicating with the POS API v2. All of the requests in this interface should use
    /// the <see cref="IPosApiEventListener" /> to return the API result and any related messages
    /// asynchronously. Any API errors should invoke the provided <see cref="IPosApiEventListener.Error" />
    /// listener.
    /// </summary>
    public interface IPosApiService
    {
        /// <summary>
        /// Set the event listener to use for receiving responses to all the requests in this service.
        /// </summary>
        /// <param name="eventListener">Event listener implementation.</param>
        public void SetEventListener(IPosApiEventListener eventListener);

        /// <summary>
        /// Set the pair secret obtained from initial pairing. See <see cref="PairingRequest" />. It is not
        /// necessary to call this post initial pairing.
        /// <param name="pairSecret">Pairing secret required to authenticate the service.</param>
        /// </summary>
        public void SetPairSecret(string pairSecret);

        /// <summary>
        /// Pair with the PIN pad. Pairing only needs to occur once and must occur before any requests can be
        /// sent to the PIN pad. Upon successful pairing the
        /// <see cref="IPosApiEventListener.PairingComplete" /> listener will be invoked.
        /// <see cref="PairingResponse.Secret" /> should be stored securely by the client. In cases where the
        /// <see cref="PosApiService" /> is initialised and the pair secret is known, it should be passed to the
        /// service using <see cref="SetPairSecret" /> to avoid unnecessary re-pairing.
        /// </summary>
        /// <param name="request">Pairing request.</param>
        public void PairingRequest(PairingRequest request);

        /// <summary>
        /// Send a transaction request. This method will invoke <see cref="IPosApiEventListener.Display" /> and
        /// <see cref="IPosApiEventListener.Receipt" /> listeners as the corresponding events are received from
        /// the PIN pad and <see cref="IPosApiEventListener.TransactionComplete" /> upon successful completion
        /// of the request.
        /// </summary>
        /// <param name="request">
        /// The following transaction models are supported:
        /// <list type="bullet">
        /// <item><description><see cref="PurchaseRequest" /></description></item>
        /// <item><description><see cref="RefundRequest" /></description></item>
        /// <item><description><see cref="CashRequest" /></description></item>
        /// <item><description><see cref="DepositRequest" /></description></item>
        /// <item><description><see cref="PreAuthRequest" /></description></item>
        /// <item><description><see cref="PreAuthExtendRequest" /></description></item>
        /// <item><description><see cref="PreAuthTopUpRequest" /></description></item>
        /// <item><description><see cref="PreAuthCancelRequest" /></description></item>
        /// <item><description><see cref="PreAuthPartialCancelRequest" /></description></item>
        /// <item><description><see cref="PreAuthCompletionRequest" /></description></item>
        /// <item><description><see cref="PreAuthInquiryRequest" /></description></item>
        /// <item><description><see cref="PreAuthSummaryRequest" /></description></item>
        /// <item><description><see cref="VoidRequest" /></description></item>
        /// </list>
        /// </param>
        /// <returns>SessionId of the request</returns>
        public Guid TransactionRequest(TransactionRequest request);

        /// <summary>
        /// Send a status request. This method will invoke the
        /// <see cref="IPosApiEventListener.StatusComplete" /> listener upon successful completion of the
        /// request.
        /// </summary>
        /// <param name="request"></param>
        /// <returns>SessionId of the request</returns>
        public Guid StatusRequest(StatusRequest request);

        /// <summary>
        /// Send a logon request. This method will invoke <see cref="IPosApiEventListener.Display" /> and
        /// <see cref="IPosApiEventListener.Receipt" /> listeners as the corresponding events are received from
        /// the PIN pad and <see cref="IPosApiEventListener.LogonComplete" /> upon successful completion
        /// of the request.
        /// </summary>
        /// <param name="request"></param>
        /// <returns>SessionId of the request</returns>
        public Guid LogonRequest(LogonRequest request);

        /// <summary>
        /// Send a settlement request. This method will invoke <see cref="IPosApiEventListener.Display" /> and
        /// <see cref="IPosApiEventListener.Receipt" /> listeners as the corresponding events are received from
        /// the PIN pad and <see cref="IPosApiEventListener.SettlementComplete" /> upon successful completion
        /// of the request.
        /// </summary>
        /// <param name="request"></param>
        /// <returns>SessionId of the request</returns>
        public Guid SettlementRequest(SettlementRequest request);

        /// <summary>
        /// Send a query card request. This method will invoke the
        /// <see cref="IPosApiEventListener.Display" /> listener as the events are received from the
        /// PIN pad and <see cref="IPosApiEventListener.QueryCardComplete" /> upon successful completion
        /// of the request.
        /// </summary>
        /// <param name="request"></param>
        /// <returns>SessionId of the request</returns>
        public Guid QueryCardRequest(QueryCardRequest request);

        /// <summary>
        /// Send a request to configure the merchant's PIN pad settings. This method will invoke the
        /// <see cref="IPosApiEventListener.ConfigureMerchantComplete" /> listener upon successful
        /// completion of this request.
        /// </summary>
        /// <param name="request"></param>
        /// <returns>SessionId of the request</returns>
        public Guid ConfigureMerchantRequest(ConfigureMerchantRequest request);

        /// <summary>
        /// Send a request to reprint a previous receipt. This method will invoke the
        /// <see cref="IPosApiEventListener.ReprintReceiptComplete" /> listener upon successful completion
        /// of this request.
        /// </summary>
        /// <param name="request"></param>
        /// <returns>SessionId of the request</returns>
        public Guid ReprintReceiptRequest(ReprintReceiptRequest request);

        /// <summary>Send a key press to the PIN pad.</summary>
        /// <param name="request"></param>
        public void SendKeyRequest(SendKeyRequest request);

        /// <summary>
        /// Retrieve all the events for a session. This method will invoke the
        /// <see cref="IPosApiEventListener.ResultComplete" /> listener upon successful completion of this
        /// request.
        /// </summary>
        /// <param name="request"></param>
        public void ResultRequest(ResultRequest request);

        /// <summary>
        /// Retrieve historical transaction results. This method will invoke the
        /// <see cref="IPosApiEventListener.RetrieveTransactionComplete"/> listener upon successful
        /// completion of this request.
        /// </summary>
        /// <param name="request"></param>
        public void RetrieveTransactionRequest(RetrieveTransactionRequest request);
    }
}