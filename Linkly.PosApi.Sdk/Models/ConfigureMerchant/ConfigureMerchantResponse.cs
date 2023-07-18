namespace Linkly.PosApi.Sdk.Models.ConfigureMerchant
{
    /// <summary>Response to a <see cref="ConfigureMerchantRequest" />.</summary>
    public class ConfigureMerchantResponse : PosApiResponseWithResult
    {
        /// <summary>Two digit merchant code. <example>00</example></summary>
        public string Merchant { get; set; } = null!;
    }
}