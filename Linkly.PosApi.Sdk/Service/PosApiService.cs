using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Web;
using Linkly.PosApi.Sdk.Common;
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
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Linkly.PosApi.Sdk.Service
{
    /// <summary>
    /// POS API service for communicating with the Cloud POS API v2. This service adopts an event-driven
    /// architecture to facilitate asynchronous communication with the API. Requests will be sent to the
    /// API in a thread and the result returned via the corresponding <see cref="IPosApiEventListener"/>
    /// method.
    ///
    /// The user of this service must provided an implementation of <see cref="IPosApiEventListener"/>
    /// which handles all the methods accordingly.
    /// 
    /// To use this service the PIN pad must first be paired using <see cref="PairingRequest"/> or if
    /// the PIN pad was previously paired use <see cref="SetPairSecret"/> to set the pairing secret.
    /// </summary>
    public class PosApiService : IPosApiService
    {
        /// <summary>Used to indicate if a request should be directed towards the POS or auth API.</summary>
        private enum ApiType
        {
            Auth,
            Pos
        }

        private const string TokensEndpoint = "/v1/tokens/cloudpos";
        private const string PairEndpoint = "/v1/pairing/cloudpos";
        private const string TransactionEndpoint = "/v2/sessions/{0}/transaction";
        private const string StatusEndpoint = "/v2/sessions/{0}/status";
        private const string LogonEndpoint = "/v2/sessions/{0}/logon";
        private const string SettlementEndpoint = "/v2/sessions/{0}/settlement";
        private const string QueryCardEndpoint = "/v2/sessions/{0}/querycard";
        private const string ConfigureMerchantEndpoint = "/v2/sessions/{0}/configuremerchant";
        private const string ReprintReceiptEndpoint = "/v2/sessions/{0}/reprintreceipt";
        private const string SendKeyEndpoint = "/v2/sessions/{0}/sendkey";
        private const string ResultEndpoint = "/v2/sessions/{0}/result?all={1}";
        private const string RetrieveTransactionEndpoint = "/v2/transaction?reference={0}&referenceType={1}";
        private const HttpStatusCode HttpStatusTooEarly = (HttpStatusCode)425;

        private readonly ILogger _logger;
        private readonly PosApiEventListenerProxy _eventListenerProxy;
        private readonly HttpClient _httpClient;
        private readonly PosVendorDetails _posVendorDetails;
        private readonly PosApiServiceOptions _options;
        private readonly JsonSerializerOptions _jsonSerializerOptions;
        private readonly ApiServiceEndpoint _serviceEndpoints;
        
        private AuthToken? _authToken;
        private string? _pairSecret;

        /// <summary>Initialise a new POS API service.</summary>
        /// <param name="logger">Optional logger to be supplied for logging and tracing.</param>
        /// <param name="httpClient">
        /// Optional client for sending requests to the API. If null a new instance will be created.
        /// </param>
        /// <param name="eventListener">Listener for all events triggered by this service.</param>
        /// <param name="serviceEndpoints">Auth and POS API endpoint URIs.</param>
        /// <param name="posVendorDetails">Identification of the POS vendor - client of this service.</param>
        /// <param name="options">Optional service options.</param>
        /// <exception cref="ArgumentNullException"></exception>
        public PosApiService(ILogger? logger, HttpClient? httpClient, IPosApiEventListener eventListener,
            ApiServiceEndpoint serviceEndpoints, PosVendorDetails posVendorDetails, PosApiServiceOptions? options = null)
        {
            _logger = logger ?? NullLogger.Instance;
            _options = options ?? new PosApiServiceOptions();

            if (serviceEndpoints is null)
            {
                _logger.LogError("{Method}: {Param} must not be null", nameof(PosApiService), nameof(serviceEndpoints));
                throw new ArgumentNullException(nameof(serviceEndpoints), "Required");
            }

            if (posVendorDetails is null)
            {
                _logger.LogError("{Method}: {Param} must not be null", nameof(PosApiService), nameof(posVendorDetails));
                throw new ArgumentNullException(nameof(posVendorDetails), "Required");
            }

            _httpClient = httpClient ?? new HttpClient();
            _eventListenerProxy = new PosApiEventListenerProxy(_logger, eventListener);
            _posVendorDetails = posVendorDetails;
            _serviceEndpoints = serviceEndpoints;

            _jsonSerializerOptions = new JsonSerializerOptions
            {
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                PropertyNameCaseInsensitive = true,
                Converters = { new JsonStringEnumMemberConverter() }
            };
        }

        /// <inheritdoc />
        public void SetEventListener(IPosApiEventListener eventListener)
        {
            _logger.LogInformation("{Method}: Setting event listener", _logger);
            _eventListenerProxy.SetEventListener(eventListener);
        }

        /// <inheritdoc />
        public void SetPairSecret(string pairSecret)
        {
            _logger.LogTrace("{Method}: Setting pair-secret", nameof(SetPairSecret));
            if (string.IsNullOrWhiteSpace(pairSecret))
            {
                _logger.LogError("{Method}: {Param} must not be null or whitespace", nameof(SetPairSecret), nameof(pairSecret));
                throw new ArgumentException("Required", nameof(pairSecret));
            }

            // Pair secret has not changed. Do nothing.
            if (string.Equals(_pairSecret, pairSecret, StringComparison.Ordinal))
            {
                _logger.LogTrace("{Method}: Attempting to set the same pair-secret, aborting", nameof(SetPairSecret));
                return;
            }

            // Clearing the auth token will trigger a re-authentication.
            _logger.LogTrace("{Method}: Clearing auth token due to pair-secret update", nameof(SetPairSecret));
            _authToken = null;
            _pairSecret = pairSecret;
        }

        /// <inheritdoc />
        public void PairingRequest(PairingRequest request)
        {
            _logger.LogInformation("{Method}: Starting", nameof(PairingRequest));
            ExecuteCommon(HandlePairingRequest, request);
        }

        /// <inheritdoc />
        public Guid TransactionRequest(TransactionRequest request)
        {
            _logger.LogInformation("{Method}: Starting", nameof(TransactionRequest));
            ValidatePairing();
            SetPosVendorDetails(request);
            return ExecuteCommon(HandleTransactionRequest, request);
        }

        private async Task HandleTransactionRequest(TransactionRequest request, Guid sessionId) =>
            await HandleCommonPosRequestAsync<TransactionRequest, TransactionResponse>(_eventListenerProxy.TransactionComplete, request,
                TransactionEndpoint, sessionId);

        /// <inheritdoc />
        public Guid StatusRequest(StatusRequest request)
        {
            _logger.LogInformation("{Method}: Starting", nameof(StatusRequest));
            ValidatePairing();
            SetPosVendorDetails(request);
            return ExecuteCommon(HandleStatusRequest, request);
        }

        /// <inheritdoc />
        public Guid LogonRequest(LogonRequest request)
        {
            _logger.LogInformation("{Method}: Starting", nameof(LogonRequest));
            ValidatePairing();
            SetPosVendorDetails(request);
            return ExecuteCommon(HandleLogonRequest, request);
        }

        /// <inheritdoc />
        public Guid SettlementRequest(SettlementRequest request)
        {
            _logger.LogInformation("{Method}: Starting", nameof(SettlementRequest));
            ValidatePairing();
            SetPosVendorDetails(request);
            return ExecuteCommon(HandleSettlementRequest, request);
        }

        /// <inheritdoc />
        public Guid QueryCardRequest(QueryCardRequest request)
        {
            _logger.LogInformation("{Method}: Starting", nameof(QueryCardRequest));
            ValidatePairing();
            SetPosVendorDetails(request);
            return ExecuteCommon(HandleQueryCardRequest, request);
        }

        /// <inheritdoc />
        public Guid ConfigureMerchantRequest(ConfigureMerchantRequest request)
        {
            _logger.LogInformation("{Method}: Starting", nameof(ConfigureMerchantRequest));
            ValidatePairing();
            SetPosVendorDetails(request);
            return ExecuteCommon(HandleConfigureMerchantRequest, request);
        }

        /// <inheritdoc />
        public Guid ReprintReceiptRequest(ReprintReceiptRequest request)
        {
            _logger.LogInformation("{Method}: Starting", nameof(ReprintReceiptRequest));
            ValidatePairing();
            SetPosVendorDetails(request);
            return ExecuteCommon(HandleReprintReceiptRequest, request);
        }

        /// <inheritdoc />
        public void SendKeyRequest(SendKeyRequest request)
        {
            _logger.LogInformation("{Method}: Starting", nameof(SendKeyRequest));
            ValidatePairing();
            SetPosVendorDetails(request);
            ExecuteCommon(HandleSendKeyRequest, request);
        }

        /// <inheritdoc />
        public void ResultRequest(ResultRequest request)
        {
            _logger.LogInformation("{Method}: Starting", nameof(ResultRequest));
            ValidatePairing();
            ExecuteCommon(HandleResultRequest, request);
        }

        /// <inheritdoc />
        public void RetrieveTransactionRequest(RetrieveTransactionRequest request)
        {
            _logger.LogInformation("{Method}: Starting", nameof(RetrieveTransactionRequest));
            ValidatePairing();
            ExecuteCommon(HandleRetrieveTransactionRequest, request);
        }

        /// <summary>Throw an <see cref="InvalidOperationException" /> if the SDK is not paired with a PIN pad.</summary>
        private void ValidatePairing([CallerMemberName] string callerMethod = "")
        {
            if (_pairSecret is null)
            {
                _logger.LogError("{Method}: Invoked without pairing", callerMethod);
                throw new InvalidOperationException("Pairing is required");
            }
        }

        private void SetPosVendorDetails(PosApiRequest request)
        {
            request.PosId = _posVendorDetails.PosId;
            request.PosName = _posVendorDetails.PosName;
            request.PosVersion = _posVendorDetails.PosVersion;
        }

        private async Task HandlePairingRequest(PairingRequest request)
        {
            _logger.LogInformation("{Method}: Pairing request for Username: {Username}", nameof(HandlePairingRequest), request.Username);

            var response = await SendRequestAsync<PairingResponse>(ApiType.Auth, HttpMethod.Post, PairEndpoint, request);

            if (response.TryGetSuccessResponse(out var pairingResponse))
            {
                _logger.LogInformation("{Method}: Request successful, updating pairing secret", nameof(PairingRequest));
                SetPairSecret(pairingResponse.Secret);
                _eventListenerProxy.PairingComplete(request, pairingResponse);
            }
        }
        
        private async Task HandleStatusRequest(StatusRequest request, Guid sessionId) =>
            await HandleCommonPosRequestAsync<StatusRequest, StatusResponse>(_eventListenerProxy.StatusComplete, request, StatusEndpoint,
                sessionId);

        private async Task HandleLogonRequest(LogonRequest request, Guid sessionId) =>
            await HandleCommonPosRequestAsync<LogonRequest, LogonResponse>(_eventListenerProxy.LogonComplete, request, LogonEndpoint, sessionId);

        private async Task HandleSettlementRequest(SettlementRequest request, Guid sessionId) =>
            await HandleCommonPosRequestAsync<SettlementRequest, SettlementResponse>(_eventListenerProxy.SettlementComplete, request,
                SettlementEndpoint, sessionId);

        private async Task HandleQueryCardRequest(QueryCardRequest request, Guid sessionId) =>
            await HandleCommonPosRequestAsync<QueryCardRequest, QueryCardResponse>(_eventListenerProxy.QueryCardComplete, request,
                QueryCardEndpoint, sessionId);

        private async Task HandleConfigureMerchantRequest(ConfigureMerchantRequest request, Guid sessionId) =>
            await HandleCommonPosRequestAsync<ConfigureMerchantRequest, ConfigureMerchantResponse>(_eventListenerProxy.ConfigureMerchantComplete,
                request, ConfigureMerchantEndpoint, sessionId);

        private async Task HandleReprintReceiptRequest(ReprintReceiptRequest request, Guid sessionId) =>
            await HandleCommonPosRequestAsync<ReprintReceiptRequest, ReprintReceiptResponse>(_eventListenerProxy.ReprintReceiptComplete, request,
                ReprintReceiptEndpoint, sessionId);

        private async Task HandleSendKeyRequest(SendKeyRequest request)
        {
            var requestUri = string.Format(SendKeyEndpoint, request.SessionId.ToString("N"));
            await SendPosRequestAsync(HttpMethod.Post, requestUri, request);
        }

        private async Task HandleResultRequest(ResultRequest request)
        {
            var responses = await SendResultRequestAsync(request, request.SessionId, true);
            _eventListenerProxy.ResultComplete(request, responses.ToList());
        }

        private async Task HandleRetrieveTransactionRequest(RetrieveTransactionRequest request)
        {
            _logger.LogInformation("{Method}: Retrieving transaction for ReferenceType: {ReferenceType}, Reference: {Reference}",
                nameof(HandleRetrieveTransactionRequest), request.ReferenceType, request.Reference);
            var requestUri = string.Format(RetrieveTransactionEndpoint,
                HttpUtility.UrlEncode(request.Reference), request.ReferenceType);
            var response = await SendPosRequestAsync<ICollection<TransactionResponse>>(HttpMethod.Get, requestUri, null, request);

            if (response.TryGetSuccessResponse(out var transactionResponses))
                _eventListenerProxy.RetrieveTransactionComplete(request, transactionResponses);
        }

        /// <summary>
        /// Execute a <see cref="IPosApiService" /> request which does not require a session ID. Validates the
        /// <paramref name="request" /> and runs <paramref name="requestHandler" /> asynchronously catching and
        /// logging exceptions thrown.
        /// </summary>
        /// <typeparam name="TRequest">Type of request to execute.</typeparam>
        /// <param name="requestHandler">Request method to execute using <paramref name="request" />.</param>
        /// <param name="request">Request to pass to <paramref name="requestHandler" />.</param>
        /// <param name="callerMethod">Ignore. Automatically populated.</param>
        private void ExecuteCommon<TRequest>(RequestHandler<TRequest> requestHandler, TRequest request,
            [CallerMemberName] string callerMethod = "")
            where TRequest : IBaseRequest, IValidatable
        {
            _ = ExecuteCommon((req, session) => requestHandler(request), request, callerMethod);
        }

        /// <summary>
        /// Execute a <see cref="IPosApiService" /> request which requires a session ID. Validates the
        /// <paramref name="request" /> and runs <paramref name="requestHandler" /> asynchronously catching and
        /// logging exceptions thrown.
        /// </summary>
        /// <typeparam name="TRequest">Type of request to execute.</typeparam>
        /// <param name="requestHandler">Request method to execute using <paramref name="request" />.</param>
        /// <param name="request">Request to pass to <paramref name="requestHandler" />.</param>
        /// <param name="callerMethod">Ignore. Automatically populated.</param>
        private Guid ExecuteCommon<TRequest>(RequestHandlerWithSession<TRequest> requestHandler, TRequest request,
            [CallerMemberName] string callerMethod = "")
            where TRequest : IBaseRequest, IValidatable
        {
            ValidateRequest(request);

            var sessionId = Guid.NewGuid();

            _ = Task.Run(async () =>
            {
                try
                {
                    await requestHandler(request, sessionId);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "{Method}: Error completing {Request}", nameof(ExecuteCommon), callerMethod);
                }
            }).ConfigureAwait(false);

            return sessionId;
        }

        /// <summary>Validate a request model and throw an exception if there are validation errors.</summary>
        /// <param name="request">Model to validate.</param>
        /// <exception cref="ArgumentException"></exception>
        private void ValidateRequest(IValidatable request)
        {
            _logger.LogTrace("{Method}: Validating request", nameof(ValidateRequest));

            var validationResult = request.Validate();
            if (validationResult.IsValid)
                return;

            var validationErrors = new StringBuilder();
            foreach (var error in validationResult.Errors)
                validationErrors.AppendLine($"'{error.PropertyName}': {error.ErrorMessage}");

            _logger.LogError("{Method}: Validation failed. {ValidationMessage}", nameof(ValidateRequest), validationErrors.ToString());
            throw new ArgumentException(validationErrors.ToString(), nameof(request));
        }

        /// <summary>
        /// Handle POS API requests by invoking the respective listeners in <see cref="IPosApiEventListener" />
        /// as responses are fetched from the result endpoint.
        /// </summary>
        /// <typeparam name="TRequest">Type of request to serialise.</typeparam>
        /// <typeparam name="TResponse">Type of response to deserialise.</typeparam>
        /// <param name="listener">listener to invoke when the final response is received.</param>
        /// <param name="request">Request body to serialise and send (as JOSN) to the API.</param>
        /// <param name="relativeUri">Relative part of the URI which specifies the API endpoint.</param>
        /// <param name="sessionId">Session ID of the request.</param>
        private async Task HandleCommonPosRequestAsync<TRequest, TResponse>(Action<Guid, TRequest, TResponse> listener,
            TRequest request, string relativeUri, Guid sessionId)
            where TRequest : PosApiRequest
            where TResponse : PosApiResponse
        {
            _logger.LogInformation("{Method}: Performing request {RequestType}. Session ID: {SessionId}",
                nameof(HandleCommonPosRequestAsync), request.GetType().Name, sessionId);

            var requestUri = string.Format(relativeUri, sessionId.ToString("N"));
            if (!(await SendPosRequestAsync(HttpMethod.Post, requestUri, request, null, sessionId)).Success)
            {
                _logger.LogError("{Method}: Request failed, aborting", nameof(HandleCommonPosRequestAsync));
                return;
            }

            _logger.LogInformation("{Method}: Request completed, fetching events", nameof(HandleCommonPosRequestAsync));

            var timer = Stopwatch.StartNew();

            while (timer.Elapsed < _options.AsyncRequestTimeout)
            {
                _logger.LogTrace("{Method}: Checking for new events", nameof(HandleCommonPosRequestAsync));
                foreach (var response in await SendResultRequestAsync(request, sessionId, false))
                    switch (response)
                    {
                        case ReceiptResponse receiptResponse:
                            _eventListenerProxy.Receipt(sessionId, request, receiptResponse);
                            break;
                        case DisplayResponse displayResponse:
                            _eventListenerProxy.Display(sessionId, request, displayResponse);
                            break;
                        case TResponse tResponse:
                            listener(sessionId, request, tResponse);
                            return; // Final result has been received.
                    }
            }

            _logger.LogError("{Method}: {TimeoutVariable} elapsed waiting for {ResponseTypeName}", nameof(HandleCommonPosRequestAsync), nameof(_options.AsyncRequestTimeout), typeof(TResponse).Name);
            _eventListenerProxy.Error(sessionId, request, new ErrorResponse(ErrorSource.Internal, "Timed out waiting for response", null));
        }

        /// <summary>Serialise and send a POS API /result request and deserialise the responses.</summary>
        /// <param name="originalRequest">
        /// Original request which initiated this request. This is provided when invoking the
        /// <see cref="IPosApiEventListener.Error" /> handler to give the caller context.
        /// </param>
        /// <param name="sessionId">Session ID </param>
        /// <param name="all"></param>
        /// <returns>POS API responses for the specified session ID.</returns>
        /// <exception cref="InvalidOperationException"></exception>
        private async Task<IEnumerable<PosApiResponse>> SendResultRequestAsync(IBaseRequest originalRequest, Guid sessionId, bool all)
        {
            _logger.LogTrace("{Method}: Getting POS API events for Session ID: {SessionId}, All: {All}",
                nameof(SendResultRequestAsync), sessionId, all);

            var requestUri = string.Format(ResultEndpoint, sessionId.ToString("D"), all);
            var apiResponse = await SendPosRequestAsync<IEnumerable<JsonNode>>(HttpMethod.Get, requestUri, null, originalRequest, sessionId);

            if (apiResponse.StatusCode == HttpStatusTooEarly)
            {
                _logger.LogTrace("{Method}: No new events from API", nameof(SendResultRequestAsync));
                return Array.Empty<PosApiResponse>();
            }

            if (!apiResponse.TryGetSuccessResponse(out var results))
            {
                _logger.LogError("{Method}: Result request unsuccessful", nameof(SendResultRequestAsync));
                throw new InvalidOperationException();
            }

            _logger.LogTrace("{Method}: Result request successful", nameof(SendResultRequestAsync));

            var responses = new List<PosApiResponse>();

            foreach (var jsonNode in results)
            {
                var responseType = (string?)jsonNode[nameof(PosApiResponse.ResponseType)] ?? "";
                _logger.LogTrace("{Method}: Deserialising response type: {ResponseType}", nameof(SendResultRequestAsync), responseType);
                PosApiResponse? posApiResponse = responseType.ToLower() switch
                {
                    Constants.ResponseType.Logon => jsonNode.Deserialize<LogonResponse>(_jsonSerializerOptions),
                    Constants.ResponseType.Status => jsonNode.Deserialize<StatusResponse>(_jsonSerializerOptions),
                    Constants.ResponseType.Display => jsonNode.Deserialize<DisplayResponse>(_jsonSerializerOptions),
                    Constants.ResponseType.Receipt => jsonNode.Deserialize<ReceiptResponse>(_jsonSerializerOptions),
                    Constants.ResponseType.ConfigureMerchant => jsonNode.Deserialize<ConfigureMerchantResponse>(_jsonSerializerOptions),
                    Constants.ResponseType.QueryCard => jsonNode.Deserialize<QueryCardResponse>(_jsonSerializerOptions),
                    Constants.ResponseType.ReprintReceipt => jsonNode.Deserialize<ReprintReceiptResponse>(_jsonSerializerOptions),
                    Constants.ResponseType.Transaction => jsonNode.Deserialize<TransactionResponse>(_jsonSerializerOptions),
                    Constants.ResponseType.Settlement => jsonNode.Deserialize<SettlementResponse>(_jsonSerializerOptions),
                    _ => throw new ArgumentOutOfRangeException($"Unexpected value of ResponseType: '{responseType}'")
                };
                responses.Add(posApiResponse!);
            }

            return responses;
        }

        /// <inheritdoc cref="SendRequestAsync" />
        private async Task<JsonApiResponse<TResponse>> SendRequestAsync<TResponse>(ApiType apiType, HttpMethod method,
            string relativeUri, IBaseRequest? request, IBaseRequest? originalRequest = null, Guid? sessionId = null)
            where TResponse : class
        {
            var response = await SendRequestAsync(apiType, method, relativeUri, request, originalRequest, sessionId);
            return new JsonApiResponse<TResponse>(response, _jsonSerializerOptions);
        }

        /// <summary>
        /// Perform a POS or auth API request and return the response and deserialised body. Also invoke the
        /// error handler if a <see cref="HttpRequestException" /> is thrown or an HTTP error response code is
        /// returned.
        /// </summary>
        /// <param name="apiType">Whether the API request is an auth or POS API request.</param>
        /// <param name="method">HTTP method to use.</param>
        /// <param name="relativeUri">Relative API endpoint path.</param>
        /// <param name="request">Request to send to API.</param>
        /// <param name="originalRequest">
        /// Optional initial request sent by the client, for use when invoking the
        /// <see cref="IPosApiEventListener.Error" /> handler. If the same as <paramref name="request" /> this
        /// can be omitted.
        /// </param>
        /// <param name="sessionId">Session ID of the originating request.</param>
        private async Task<ApiResponse> SendRequestAsync(ApiType apiType, HttpMethod method,
            string relativeUri, IBaseRequest? request, IBaseRequest? originalRequest = null, Guid? sessionId = null)
        {
            originalRequest ??= request ?? throw new InvalidOperationException();

            var requestUri = apiType == ApiType.Auth
                ? new Uri(_serviceEndpoints.AuthApiBaseUri, relativeUri)
                : new Uri(_serviceEndpoints.PosApiBaseUri, relativeUri);

            try
            {
                _logger.LogTrace("{Method}: Sending {HttpMethod} request to endpoint: '{Endpoint}'",
                    nameof(SendRequestAsync), method, requestUri.AbsoluteUri);

                var httpRequest = new HttpRequestMessage(method, requestUri);
                if (apiType == ApiType.Pos)
                    httpRequest.Headers.Authorization = _authToken?.GetAuthenticationHeaderValue();

                if (request is null)
                    _logger.LogTrace("{Method}: {Param} is null, HTTP content empty", nameof(SendRequestAsync), nameof(request));
                else
                    httpRequest.Content = JsonContent.Create(request.ToDto(), options: _jsonSerializerOptions);

                _logger.LogTrace("{Method}: Sending HTTP request", nameof(SendRequestAsync));
                var httpResponse = await _httpClient.SendAsync(httpRequest).ConfigureAwait(false);
                _logger.LogTrace("{Method}: Reading HTTP response", nameof(SendRequestAsync));
                var httpContent = await httpResponse.Content.ReadAsStringAsync().ConfigureAwait(false);

                if (httpResponse.IsSuccessStatusCode)
                    _logger.LogTrace("{Method}: HTTP status code: {StatusCode}", nameof(SendRequestAsync), httpResponse.StatusCode);
                else
                    _logger.LogError("{Method}: HTTP status code: {StatusCode}, Message: '{Body}'",
                        nameof(SendRequestAsync), httpResponse.StatusCode, httpContent);

                if (!httpResponse.IsSuccessStatusCode && httpResponse.StatusCode != HttpStatusTooEarly)
                {
                    _logger.LogError("{Method}: Request unsuccessful", nameof(SendRequestAsync));
                    _eventListenerProxy.Error(sessionId, originalRequest, new ErrorResponse(ErrorSource.API, httpContent, httpResponse.StatusCode));
                }

                return new ApiResponse(httpResponse.IsSuccessStatusCode, httpResponse.StatusCode, httpContent);
            }
            catch (HttpRequestException ex)
            {
                _eventListenerProxy.Error(sessionId, originalRequest, new ErrorResponse(ErrorSource.Internal, "Error completing request", null, ex));
                throw;
            }
        }

        /// <inheritdoc cref="SendPosRequestAsync" />
        private async Task<JsonApiResponse<TResponse>> SendPosRequestAsync<TResponse>(HttpMethod method, string relativeUri,
            IBaseRequest? request, IBaseRequest? originalRequest = null, Guid? sessionId = null)
            where TResponse : class
        {
            var response = await SendPosRequestAsync(method, relativeUri, request, originalRequest, sessionId);
            return new JsonApiResponse<TResponse>(response, _jsonSerializerOptions);
        }

        /// <summary>Serialise and send a POS API request and return the response.</summary>
        /// <param name="method">HTTP method of request.</param>
        /// <param name="relativeUri">Relative part of the URI which specifies the API endpoint.</param>
        /// <param name="request">Request body to serialise and send (as JOSN) to the API.</param>
        /// <param name="originalRequest">
        /// Optional initial request sent by the client, for use when invoking the
        /// <see cref="IPosApiEventListener.Error" /> handler. If the same as <paramref name="request" /> this
        /// can be omitted.
        /// </param>
        /// <param name="sessionId">Session ID of the originating request.</param>
        /// <exception cref="InvalidOperationException"></exception>
        private async Task<ApiResponse> SendPosRequestAsync(HttpMethod method, string relativeUri,
            IBaseRequest? request, IBaseRequest? originalRequest = null, Guid? sessionId = null)
        {
            _logger.LogTrace("{Method}: Performing POS API request, Method: {HttpMethod}, Uri: {Uri}",
                nameof(SendPosRequestAsync), method, relativeUri);

            originalRequest ??= request ?? throw new InvalidOperationException();

            if (_authToken?.IsExpiringSoon ?? true)
            {
                _logger.LogTrace("{Method}: Token expired, authenticating", nameof(SendPosRequestAsync));

                var response = await SendRequestAsync<TokenResponse>(
                    ApiType.Auth, HttpMethod.Post, TokensEndpoint, GetTokenRequest(), originalRequest, sessionId);

                if (!response.TryGetSuccessResponse(out var tokenResponse))
                {
                    _logger.LogError("{Method}: Authentication failed, aborting request", nameof(SendPosRequestAsync));
                    throw new InvalidOperationException();
                }

                _logger.LogTrace("{Method}: Authentication successful, updating token", nameof(SendPosRequestAsync));
                _authToken = new AuthToken(tokenResponse.Token, DateTime.UtcNow.AddSeconds(tokenResponse.ExpirySeconds));
            }
            else
            {
                _logger.LogTrace("{Method}: Token not expiring soon, authentication not required", nameof(SendPosRequestAsync));
            }

            var posApiResponse = await SendRequestAsync(ApiType.Pos, method, relativeUri, request, originalRequest, sessionId);

            if (posApiResponse.StatusCode == HttpStatusCode.Unauthorized)
            {
                _logger.LogTrace("{Method}: Clearing auth token due to failed authentication", nameof(SendPosRequestAsync));
                _authToken = null;
            }

            return posApiResponse;
        }

        /// <summary>Create a new token request to authenticate the <see cref="PosApiService" />.</summary>
        private TokenRequest GetTokenRequest() =>
            new TokenRequest
            {
                PosId = _posVendorDetails.PosId,
                PosName = _posVendorDetails.PosName,
                PosVendorId = _posVendorDetails.PosVendorId,
                PosVersion = _posVendorDetails.PosVersion,
                Secret = _pairSecret ?? ""
            };

        /// <summary>Request handler delegate which requires a session ID.</summary>
        private delegate Task RequestHandlerWithSession<in TRequest>(TRequest request, Guid sessionId);

        /// <summary>Request handler delegate which does not require a session ID.</summary>
        private delegate Task RequestHandler<in TRequest>(TRequest request);
    }
}