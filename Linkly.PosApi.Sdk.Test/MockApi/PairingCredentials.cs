namespace Linkly.PosApi.Sdk.UnitTest.MockApi;

/// <summary>Credentials to accept for pairing using the <see cref="MockApiHttpMessageHandler" />.</summary>
internal class PairingCredentials
{
    public PairingCredentials(string username, string password, string pairCode)
    {
        Username = username;
        Password = password;
        PairCode = pairCode;
    }

    public string Username { get; set; }
    public string Password { get; set; }
    public string PairCode { get; set; }
}