using System.Diagnostics;
using System.Net.Http.Headers;
using Linkly.PosApi.Sdk.Common;
using Linkly.PosApi.Sdk.UnitTest.Common;
using Linkly.PosApi.Sdk.UnitTest.MockApi.Models;
using Microsoft.Extensions.Logging;
using Xunit.Abstractions;

namespace Linkly.PosApi.Sdk.UnitTest.MockApi;

/// <summary>Message handler to simulate Auth API v1 and POS API V2 responses.</summary>
internal sealed class MockApiHttpMessageHandler : HttpMessageHandlerBase
{
    private const string ResultEndpointUriTemplate = "/v2/sessions/{0}/result";

    private readonly ILogger _logger;
    private readonly RequestMapper<RequestHandler> _requestHandlers;

    /// <summary>This field is used to configure the mock parameters.</summary>
    public readonly MockApiHttpMessageHandlerOptions Options;

    /// <summary>This field is inspected by the tests to see the internal state of the mock.</summary>
    public readonly MockApiHttpMessageHandlerSession Session;

    public MockApiHttpMessageHandler(ITestOutputHelper testOutput, MockApiHttpMessageHandlerOptions options)
    {
        _logger = new TestOutputLogger(nameof(MockApiHttpMessageHandler), testOutput);
        Options = options;
        Session = new MockApiHttpMessageHandlerSession();

        _requestHandlers = new RequestMapper<RequestHandler>(new KeyValuePair<string, RequestHandler>[]
        {
            new("^/v1/pairing/cloudpos", HandlePairing),
            new("^/v1/tokens/cloudpos", HandleToken),
            new("/v2/sessions/(?<sessionid>.+)/transaction", HandleTransaction),
            new("/v2/sessions/(?<sessionid>.+)/logon", HandleLogon),
            new("/v2/sessions/(?<sessionid>.+)/settlement", HandleSettlement),
            new("/v2/sessions/(?<sessionid>.+)/status", HandleStatus),
            new("/v2/sessions/(?<sessionid>.+)/querycard", HandleQueryCard),
            new("/v2/sessions/(?<sessionid>.+)/configuremerchant", HandleConfigureMerchant),
            new("/v2/sessions/(?<sessionid>.+)/reprintreceipt", HandleReprintReceipt),
            new("/v2/sessions/(?<sessionid>.+)/sendkey", HandleSendKey),
            new("/v2/sessions/(?<sessionid>.+)/result", HandleResult),
            new("/v2/transaction", HandleRetrieveTransaction)
        });
    }

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        if (request.RequestUri is null)
        {
            _logger.LogError("{Method}: URI cannot be empty", nameof(SendAsync));
            throw new HttpRequestException();
        }

        if (!_requestHandlers.TryGetHandler(request.RequestUri, out var handler, out var @params))
        {
            _logger.LogError("{Method}: Did not find a request handler for URI {RequestUri}", nameof(SendAsync), request.RequestUri);
            return Task.FromResult(NotFound());
        }

        try
        {
            var httpResponse = handler(request, @params);
            return Task.FromResult(httpResponse);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{Method}: Exception thrown in handler {Handler}", nameof(SendAsync), handler.Method.Name);
            throw;
        }
    }

    /// <summary>Handle /v1/pairing/cloudpos endpoint.</summary>
    private HttpResponseMessage HandlePairing(HttpRequestMessage httpRequest, IDictionary<string, string> @params)
    {
        if (Options.PairRequestException is not null) throw Options.PairRequestException;
        if (Options.PairRequestError is not null) return Options.PairRequestError;
        if (!ValidateUri(httpRequest.RequestUri, ApiType.Auth)) return NotFound();

        var request = DeserialiseAsync<PairingRequest>(httpRequest);

        if (!ValidateProperty(Options.PairingCredentials.Username, request.Username, nameof(request.Username), StringComparer.Ordinal)
            || !ValidateProperty(Options.PairingCredentials.Password, request.Password, nameof(request.Password), StringComparer.Ordinal)
            || !ValidateProperty(Options.PairingCredentials.PairCode, request.PairCode, nameof(request.PairCode), StringComparer.Ordinal))
            return BadRequest();

        var sessionId = Guid.NewGuid();
        Session.Requests.AddRequest(sessionId, request);

        Session.PairSecret = Utils.GenRandomString(128);
        var response = new PairingResponse { Secret = Session.PairSecret };
        Session.Requests.AddResponse(sessionId, response);

        return Ok(response);
    }

    /// <summary>Handle /v1/tokens/cloudpos endpoint.</summary>
    private HttpResponseMessage HandleToken(HttpRequestMessage httpRequest, IDictionary<string, string> @params)
    {
        if (Options.AuthRequestException is not null) throw Options.AuthRequestException;
        if (Options.AuthRequestError is not null) return Options.AuthRequestError;
        if (!ValidateUri(httpRequest.RequestUri, ApiType.Auth)) return NotFound();

        var request = DeserialiseAsync<TokenRequest>(httpRequest);

        if (!ValidateProperty(Session.PairSecret, request.Secret, nameof(request.Secret), StringComparer.Ordinal))
            return Unauthorized();

        if (!ValidateProperty(Options.PosVendorDetails.PosId, request.PosId, nameof(request.PosId))
            || !ValidateProperty(Options.PosVendorDetails.PosVendorId, request.PosVendorId, nameof(request.PosVendorId))
            || !ValidateProperty(Options.PosVendorDetails.PosName, request.PosName, nameof(request.PosName), StringComparer.Ordinal)
            || !ValidateProperty(Options.PosVendorDetails.PosVersion, request.PosVersion, nameof(request.PosVersion),
                StringComparer.Ordinal))
            return BadRequest();

        var sessionId = Guid.NewGuid();
        Session.Requests.AddRequest(sessionId, request);

        var authToken = new AuthToken(Utils.GenRandomString(256), DateTime.UtcNow.Add(Options.TokenLeasePeriod));
        Session.AuthTokens.Add(authToken);
        var response = new TokenResponse { Token = authToken.Token, ExpirySeconds = (int)Options.TokenLeasePeriod.TotalSeconds };
        Session.Requests.AddResponse(sessionId, response);

        return Ok(response);
    }

    /// <summary>Handle /v2/sessions/{sessionid}/transaction endpoint.</summary>
    private HttpResponseMessage HandleTransaction(HttpRequestMessage httpRequest, IDictionary<string, string> @params)
    {
        if (Options.PosRequestException is not null) throw Options.PosRequestException;
        if (Options.PosRequestError is not null) return Options.PosRequestError;
        if (!ValidateUri(httpRequest.RequestUri, ApiType.Pos)) return NotFound();
        if (!ValidateAuthHeader(httpRequest.Headers.Authorization)) return Unauthorized();

        var request = DeserialiseRequestDtoAsync<TransactionRequest>(httpRequest);
        var sessionId = Guid.Parse(@params["sessionid"]);
        Session.Requests.AddRequest(sessionId, request);

        new Thread(() =>
        {
            Thread.Sleep(Options.PinPadDelay);
            Session.Requests.AddResponse(sessionId, ResponseBuilder.GetDisplayResponse("Swipe Card"));
            Thread.Sleep(Options.PinPadDelay);
            Session.Requests.AddResponse(sessionId, ResponseBuilder.GetDisplayResponse("Processing"));
            Thread.Sleep(Options.PinPadDelay);
            Session.Requests.AddResponse(sessionId, ResponseBuilder.GetDisplayResponse("Ready"));
            Thread.Sleep(Options.PinPadDelay);
            Session.Requests.AddResponse(sessionId, ResponseBuilder.GetReceiptResponse("Txn Receipt"));
            Thread.Sleep(Options.PinPadDelay);
            Session.Requests.AddResponse(sessionId, ResponseBuilder.GetTransactionResponse(request));
        }).Start();

        return Ok(new IntermediateResponse(sessionId, ResultEndpointUriTemplate));
    }

    /// <summary>Handle /v2/sessions/{sessionid}/logon endpoint.</summary>
    private HttpResponseMessage HandleLogon(HttpRequestMessage httpRequest, IDictionary<string, string> @params)
    {
        if (Options.PosRequestException is not null) throw Options.PosRequestException;
        if (Options.PosRequestError is not null) return Options.PosRequestError;
        if (!ValidateUri(httpRequest.RequestUri, ApiType.Pos)) return NotFound();
        if (!ValidateAuthHeader(httpRequest.Headers.Authorization)) return Unauthorized();

        var request = DeserialiseRequestDtoAsync<LogonRequest>(httpRequest);
        var sessionId = Guid.Parse(@params["sessionid"]);
        Session.Requests.AddRequest(sessionId, request);

        new Thread(() =>
        {
            Thread.Sleep(Options.PinPadDelay);
            Session.Requests.AddResponse(sessionId, ResponseBuilder.GetDisplayResponse("Logon"));
            Thread.Sleep(Options.PinPadDelay);
            Session.Requests.AddResponse(sessionId, ResponseBuilder.GetReceiptResponse("Host Sign On"));
            Thread.Sleep(Options.PinPadDelay);
            Session.Requests.AddResponse(sessionId, ResponseBuilder.GetLogonResponse());
        }).Start();

        return Ok(new IntermediateResponse(sessionId, ResultEndpointUriTemplate));
    }

    /// <summary>Handle /v2/sessions/{sessionid}/settlement endpoint.</summary>
    private HttpResponseMessage HandleSettlement(HttpRequestMessage httpRequest, IDictionary<string, string> @params)
    {
        if (Options.PosRequestException is not null) throw Options.PosRequestException;
        if (Options.PosRequestError is not null) return Options.PosRequestError;
        if (!ValidateUri(httpRequest.RequestUri, ApiType.Pos)) return NotFound();
        if (!ValidateAuthHeader(httpRequest.Headers.Authorization)) return Unauthorized();

        var request = DeserialiseRequestDtoAsync<SettlementRequest>(httpRequest);
        var sessionId = Guid.Parse(@params["sessionid"]);
        Session.Requests.AddRequest(sessionId, request);

        new Thread(() =>
        {
            Thread.Sleep(Options.PinPadDelay);
            Session.Requests.AddResponse(sessionId, ResponseBuilder.GetDisplayResponse("Settlement"));
            Thread.Sleep(Options.PinPadDelay);
            Session.Requests.AddResponse(sessionId, ResponseBuilder.GetReceiptResponse("Settlement Receipt"));
            Thread.Sleep(Options.PinPadDelay);
            Session.Requests.AddResponse(sessionId, ResponseBuilder.GetSettlementResponse());
        }).Start();

        return Ok(new IntermediateResponse(sessionId, ResultEndpointUriTemplate));
    }

    /// <summary>Handle /v2/sessions/{sessionid}/status endpoint.</summary>
    private HttpResponseMessage HandleStatus(HttpRequestMessage httpRequest, IDictionary<string, string> @params)
    {
        if (Options.PosRequestException is not null) throw Options.PosRequestException;
        if (Options.PosRequestError is not null) return Options.PosRequestError;
        if (!ValidateUri(httpRequest.RequestUri, ApiType.Pos)) return NotFound();
        if (!ValidateAuthHeader(httpRequest.Headers.Authorization)) return Unauthorized();

        var request = DeserialiseRequestDtoAsync<StatusRequest>(httpRequest);
        var sessionId = Guid.Parse(@params["sessionid"]);
        Session.Requests.AddRequest(sessionId, request);

        new Thread(() =>
        {
            Thread.Sleep(Options.PinPadDelay);
            Session.Requests.AddResponse(sessionId, ResponseBuilder.GetStatusResponse());
        }).Start();

        return Ok(new IntermediateResponse(sessionId, ResultEndpointUriTemplate));
    }

    /// <summary>Handle /v2/sessions/{sessionid}/querycard endpoint.</summary>
    private HttpResponseMessage HandleQueryCard(HttpRequestMessage httpRequest, IDictionary<string, string> @params)
    {
        if (Options.PosRequestException is not null) throw Options.PosRequestException;
        if (Options.PosRequestError is not null) return Options.PosRequestError;
        if (!ValidateUri(httpRequest.RequestUri, ApiType.Pos)) return NotFound();
        if (!ValidateAuthHeader(httpRequest.Headers.Authorization)) return Unauthorized();

        var request = DeserialiseRequestDtoAsync<QueryCardRequest>(httpRequest);
        var sessionId = Guid.Parse(@params["sessionid"]);
        Session.Requests.AddRequest(sessionId, request);

        new Thread(() =>
        {
            Thread.Sleep(Options.PinPadDelay);
            Session.Requests.AddResponse(sessionId, ResponseBuilder.GetDisplayResponse("Swipe Card"));
            Thread.Sleep(Options.PinPadDelay);
            Session.Requests.AddResponse(sessionId, ResponseBuilder.GetQueryCardResponse());
        }).Start();

        return Ok(new IntermediateResponse(sessionId, ResultEndpointUriTemplate));
    }

    /// <summary>Handle /v2/sessions/{sessionid}/configuremerchant endpoint.</summary>
    private HttpResponseMessage HandleConfigureMerchant(HttpRequestMessage httpRequest, IDictionary<string, string> @params)
    {
        if (Options.PosRequestException is not null) throw Options.PosRequestException;
        if (Options.PosRequestError is not null) return Options.PosRequestError;
        if (!ValidateUri(httpRequest.RequestUri, ApiType.Pos)) return NotFound();
        if (!ValidateAuthHeader(httpRequest.Headers.Authorization)) return Unauthorized();

        var request = DeserialiseRequestDtoAsync<ConfigureMerchantRequest>(httpRequest);
        var sessionId = Guid.Parse(@params["sessionid"]);
        Session.Requests.AddRequest(sessionId, request);

        new Thread(() =>
        {
            Thread.Sleep(Options.PinPadDelay);
            Session.Requests.AddResponse(sessionId, ResponseBuilder.GetConfigureMerchantResponse());
        }).Start();

        return Ok(new IntermediateResponse(sessionId, ResultEndpointUriTemplate));
    }

    /// <summary>Handle /v2/sessions/{sessionid}/reprintreceipt endpoint.</summary>
    private HttpResponseMessage HandleReprintReceipt(HttpRequestMessage httpRequest, IDictionary<string, string> @params)
    {
        if (Options.PosRequestException is not null) throw Options.PosRequestException;
        if (Options.PosRequestError is not null) return Options.PosRequestError;
        if (!ValidateUri(httpRequest.RequestUri, ApiType.Pos)) return NotFound();
        if (!ValidateAuthHeader(httpRequest.Headers.Authorization)) return Unauthorized();

        var request = DeserialiseRequestDtoAsync<ReprintReceiptRequest>(httpRequest);
        var sessionId = Guid.Parse(@params["sessionid"]);
        Session.Requests.AddRequest(sessionId, request);

        new Thread(() =>
        {
            Thread.Sleep(Options.PinPadDelay);
            Session.Requests.AddResponse(sessionId, ResponseBuilder.GetReprintReceiptResponse());
        }).Start();

        return Ok(new IntermediateResponse(sessionId, ResultEndpointUriTemplate));
    }

    /// <summary>Handle /v2/sessions/{sessionid}/sendkey endpoint.</summary>
    private HttpResponseMessage HandleSendKey(HttpRequestMessage httpRequest, IDictionary<string, string> @params)
    {
        if (Options.PosRequestException is not null) throw Options.PosRequestException;
        if (Options.PosRequestError is not null) return Options.PosRequestError;
        if (!ValidateUri(httpRequest.RequestUri, ApiType.Pos)) return NotFound();
        if (!ValidateAuthHeader(httpRequest.Headers.Authorization)) return Unauthorized();

        var request = DeserialiseRequestDtoAsync<SendKeyRequest>(httpRequest);
        var sessionId = Guid.Parse(@params["sessionid"]);
        Session.Requests.AddRequest(sessionId, request);

        new Thread(() => { Session.Requests.AddResponse(sessionId, ResponseBuilder.GetReprintReceiptResponse()); }).Start();

        return Ok(new IntermediateResponse(sessionId, ResultEndpointUriTemplate));
    }

    /// <summary>Handle /result endpoint.</summary>
    private HttpResponseMessage HandleResult(HttpRequestMessage httpRequest, IDictionary<string, string> @params)
    {
        if (Options.ResultRequestException is not null) throw Options.ResultRequestException;
        if (Options.ResultRequestError is not null) return Options.ResultRequestError;
        if (!ValidateUri(httpRequest.RequestUri, ApiType.Pos)) return NotFound();
        if (!ValidateAuthHeader(httpRequest.Headers.Authorization)) return Unauthorized();

        var sessionId = Guid.Parse(@params["sessionid"]);
        var query = QueryStringParser.Parse(httpRequest.RequestUri?.Query);
        var all = string.Equals(query.GetValueOrDefault("all"), "true", StringComparison.OrdinalIgnoreCase);

        var sw = Stopwatch.StartNew();

        if (all)
        {
            if (!Session.Requests.TryGetSession(sessionId, out _, out var responses))
                return NotFound();

            var posApiResponses = responses.OfType<PosApiResponse>().ToList();

            return posApiResponses.Any()
                ? Ok(posApiResponses.Select(resp => (object)resp).ToArray())
                : TooEarly();
        }

        do
        {
            if (!Session.Requests.TryGetSession(sessionId, out _, out var responses))
                return NotFound();

            var posApiResponses = responses.OfType<PosApiResponse>().ToList();

            var unsentResponses = posApiResponses.Where(response => !response.Consumed).ToList();
            if (!unsentResponses.Any())
            {
                Thread.Sleep(10);
                continue;
            }

            unsentResponses.ForEach(resp => resp.Consumed = true);

            return Ok(unsentResponses.Select(resp => (object)resp).ToArray());
        } while (sw.Elapsed <= Options.ResultTimeout);

        return TooEarly();
    }

    /// <summary>Handle /transaction endpoint.</summary>
    private HttpResponseMessage HandleRetrieveTransaction(HttpRequestMessage httpRequest, IDictionary<string, string> @params)
    {
        const string referenceQueryKey = "Reference";
        const string referenceTypeQueryKey = "ReferenceType";

        if (Options.PosRequestException is not null) throw Options.PosRequestException;
        if (Options.PosRequestError is not null) return Options.PosRequestError;
        if (!ValidateUri(httpRequest.RequestUri, ApiType.Pos)) return NotFound();
        if (!ValidateAuthHeader(httpRequest.Headers.Authorization)) return Unauthorized();

        var query = QueryStringParser.Parse(httpRequest.RequestUri?.Query);
        var referenceType = query.GetValueOrDefault(referenceTypeQueryKey);
        var reference = query.GetValueOrDefault(referenceQueryKey);

        var retrievedTransactions = Session.Requests
            .SelectMany(req => req.Responses).OfType<TransactionResponse>()
            .Where(resp => referenceType?.ToLower() switch
            {
                "rrn" => string.Equals(resp.RRN, reference, StringComparison.OrdinalIgnoreCase),
                _ => string.Equals(resp.TxnRef, reference, StringComparison.OrdinalIgnoreCase)
            }).ToArray();

        if (retrievedTransactions.Any())
            return Ok(retrievedTransactions);

        _logger.LogError("{Method}: transaction reference not found, {ReferenceTypeKey}: {ReferenceType}, {ReferenceKey}: {Reference}",
            nameof(HandleRetrieveTransaction), referenceTypeQueryKey, referenceType, referenceQueryKey, reference);
        return NotFound();
    }

    /// <summary>
    /// Deserialise the content of <see cref="httpRequest" /> which is wrapped within a json "request"
    /// object into <typeparamref name="TRequest" />.
    /// </summary>
    private TRequest DeserialiseRequestDtoAsync<TRequest>(HttpRequestMessage httpRequest)
    {
        var requestDto = DeserialiseAsync<RequestDto<TRequest>>(httpRequest);
        return requestDto.Request ?? throw new InvalidOperationException();
    }

    /// <summary>
    /// Returns a bool indicating whether a <see cref="requestUri" /> is valid for the
    /// <see cref="ApiType" />.
    /// </summary>
    /// <param name="requestUri">Request URI.</param>
    /// <param name="apiType">Type of API request.</param>
    private bool ValidateUri(Uri? requestUri, ApiType apiType)
    {
        if (requestUri is null)
        {
            _logger.LogError("{Method}: {Param} cannot be null", nameof(ValidateUri), nameof(requestUri));
            return false;
        }

        if (apiType == ApiType.Auth && !Options.ServiceEndpoints.AuthApiBaseUri.IsBaseOf(requestUri))
        {
            _logger.LogError("{Method}: Request URI {RequestURI} should be a base of {AuthApiBaseUri}",
                nameof(ValidateUri), requestUri, Options.ServiceEndpoints.AuthApiBaseUri);
            return false;
        }

        if (apiType == ApiType.Pos && !Options.ServiceEndpoints.PosApiBaseUri.IsBaseOf(requestUri))
        {
            _logger.LogError("{Method}: Request URI {RequestURI} should be a base of {PosApiBaseUri}",
                nameof(ValidateUri), requestUri, Options.ServiceEndpoints.PosApiBaseUri);
            return false;
        }

        return true;
    }

    /// <summary>
    /// Returns a bool indicating whether an <see cref="authHeader" /> contains a valid bearer
    /// authorisation token.
    /// </summary>
    /// <param name="authHeader">Authentication header to validate.</param>
    private bool ValidateAuthHeader(AuthenticationHeaderValue? authHeader)
    {
        if (authHeader is null)
        {
            _logger.LogError("{Method}: {Param} cannot be null", nameof(ValidateAuthHeader), nameof(authHeader));
            return false;
        }

        var now = DateTime.UtcNow;

        if (Session.AuthTokens.Any(authToken =>
                string.Equals(authHeader.Scheme, "bearer", StringComparison.OrdinalIgnoreCase)
                && string.Equals(authToken.Token, authHeader.Parameter, StringComparison.Ordinal)
                && now < authToken.Expiry))
            return true;

        _logger.LogError("{Method}: Request does not contain the expected authentication header {HttpRequest}",
            nameof(ValidateUri), authHeader);

        return false;
    }

    /// <summary>
    /// Returns a <see cref="bool" /> which indicates whether <see cref="expectedValue" /> matches
    /// <see cref="actualValue" /> and logs an error if they do not match.
    /// </summary>
    /// <param name="expectedValue">Expected value.</param>
    /// <param name="actualValue">Actual value.</param>
    /// <param name="name">Name of property to use in error message.</param>
    /// <param name="comparer">Optional comparer to use or else the default Equals method is used.</param>
    /// <returns>Result to indicate whether the properties match.</returns>
    private bool ValidateProperty<TProperty>(TProperty? expectedValue, TProperty? actualValue, string name,
        IComparer<TProperty>? comparer = null)
    {
        if (comparer is not null && comparer.Compare(expectedValue, actualValue) == 0)
            return true;
        if (comparer is null && Equals(expectedValue, actualValue))
            return true;

        _logger.LogError("{Method}: {PropertyName} expected '{ExpectedValue}', actual '{ActualValue}'",
            nameof(ValidateProperty), name, expectedValue, actualValue);

        return false;
    }

    private delegate HttpResponseMessage RequestHandler(HttpRequestMessage httpRequest, IDictionary<string, string> @params);

    private enum ApiType
    {
        Auth,
        Pos
    }
}