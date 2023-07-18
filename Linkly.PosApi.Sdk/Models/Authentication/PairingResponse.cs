namespace Linkly.PosApi.Sdk.Models.Authentication
{
    /// <summary>Response to a <see cref="PairingRequest" />.</summary>
    public class PairingResponse : IBaseResponse
    {
        /// <summary>Non-expiring secret the POS uses to request an auth token via <see cref="TokenRequest" />.</summary>
        public string Secret { get; set; } = null!;
    }
}