using Linkly.PosApi.Sdk.Service;

namespace Linkly.PosApi.Sdk.UnitTest.MockApi;

/// <summary>Options for controlling the <see cref="MockApiHttpMessageHandler" /> behaviour.</summary>
internal class MockApiHttpMessageHandlerOptions
{
    public MockApiHttpMessageHandlerOptions(ApiServiceEndpoint serviceEndpoints, PairingCredentials pairingCredentials,
        PosVendorDetails posVendorDetails)
    {
        ServiceEndpoints = serviceEndpoints;
        PairingCredentials = pairingCredentials;
        PosVendorDetails = posVendorDetails;
    }

    /// <summary>Endpoints the <see cref="MockApiHttpMessageHandler" /> expects to receive requests on.</summary>
    public ApiServiceEndpoint ServiceEndpoints { get; set; }

    /// <summary>Credentials the <see cref="MockApiHttpMessageHandler" /> expects for pairing.</summary>
    public PairingCredentials PairingCredentials { get; set; }

    /// <summary>POS vendor details to expect when performing a token request.</summary>
    public PosVendorDetails PosVendorDetails { get; set; }

    /// <summary>Configure how long a token is valid for.</summary>
    public TimeSpan TokenLeasePeriod { get; set; } = TimeSpan.FromHours(6);

    /// <summary>Simulate a delay from all PIN pad operations.</summary>
    public TimeSpan PinPadDelay { get; set; } = TimeSpan.Zero;

    /// <summary>
    /// How long the should the /result endpoint wait for a response before returning a 425 (too
    /// early) response.
    /// </summary>
    public TimeSpan ResultTimeout { get; set; } = TimeSpan.FromSeconds(1);

    /// <summary>If not null the following exception will be thrown in the pairing request handler.</summary>
    public Exception? PairRequestException { get; set; }

    /// <summary>If not null the following exception will be thrown in the auth request handler.</summary>
    public Exception? AuthRequestException { get; set; }

    /// <summary>If not null the following exception will be thrown in the POS API request handlers.</summary>
    public Exception? PosRequestException { get; set; }

    /// <summary>If not null the following exception will be thrown in the result request handler.</summary>
    public Exception? ResultRequestException { get; set; }

    /// <summary>If not null the following HTTP error will be returned by the pairing request handler.</summary>
    public HttpResponseMessage? PairRequestError { get; set; }

    /// <summary>If not null the following HTTP error will be returned by the auth request handler.</summary>
    public HttpResponseMessage? AuthRequestError { get; set; }

    /// <summary>If not null the following HTTP error will be returned by the POS API request handlers.</summary>
    public HttpResponseMessage? PosRequestError { get; set; }

    /// <summary>If not null the following HTTP error will be returned by the result request handler.</summary>
    public HttpResponseMessage? ResultRequestError { get; set; }
}