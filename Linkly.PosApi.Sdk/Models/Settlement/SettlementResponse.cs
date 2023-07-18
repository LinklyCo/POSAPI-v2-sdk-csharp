namespace Linkly.PosApi.Sdk.Models.Settlement
{
    public class SettlementResponse : PosApiResponseWithResult
    {
        /// <summary>Two digit merchant code</summary>
        /// <example>00</example>
        public string Merchant { get; set; } = null!;

        /// <summary>Settlement data</summary>
        /// <example>
        /// 000000002138VISA                000000100001000000100001000000100001+000000300003DEBIT
        /// 000000100001000000100001000000100001+000000300003069TOTAL
        /// 000000300001000000300001000000300001+000000900009
        /// </example>
        public string SettlementData { get; set; } = null!;
    }
}