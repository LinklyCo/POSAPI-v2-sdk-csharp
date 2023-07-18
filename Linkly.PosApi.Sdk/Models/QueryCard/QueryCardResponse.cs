namespace Linkly.PosApi.Sdk.Models.QueryCard
{
    /// <summary>Response to a <see cref="QueryCardRequest" />.</summary>
    public class QueryCardResponse : PosApiResponseWithResult
    {
        /// <summary>Two digit merchant code. <example>00</example></summary>
        public string Merchant { get; set; } = null!;

        /// <summary>Indicates if track 1 was read.</summary>
        public bool IsTrack1Available { get; set; }

        /// <summary>Indicates if track 2 was read.</summary>
        public bool IsTrack2Available { get; set; }

        /// <summary>Indicates if track 3 was read.</summary>
        public bool IsTrack3Available { get; set; }

        /// <summary>Data encoded on Track1 of the card.</summary>
        public string Track1 { get; set; } = null!;

        /// <summary>Data encoded on Track2 of the card.</summary>
        public string Track2 { get; set; } = null!;

        /// <summary>Data encoded on Track3 of the card.</summary>
        public string Track3 { get; set; } = null!;

        /// <summary>BIN number of the card. For a complete list refer to the API documentation.</summary>
        /// <remarks>
        /// <list type="table">
        /// <listheader><term>Card BIN</term><description>Card Type</description></listheader>
        /// <item><term>0</term><description>Unknown</description></item>
        /// <item><term>1</term><description>Debit</description></item>
        /// <item><term>2</term><description>Bankcard</description></item>
        /// <item><term>3</term><description>Mastercard</description></item>
        /// <item><term>4</term><description>Visa</description></item>
        /// <item><term>5</term><description>American Express</description></item>
        /// <item><term>6</term><description>Diner Club</description></item>
        /// <item><term>7</term><description>JCB</description></item>
        /// <item><term>8</term><description>Label Card</description></item>
        /// <item><term>9</term><description>JCB</description></item>
        /// <item><term>11</term><description>JCB</description></item>
        /// <item><term>12</term><description>Other</description></item>
        /// </list>
        /// </remarks>
        public string CardName { get; set; } = null!;

        /// <summary>Account type selected.</summary>
        public AccountType AccountType { get; set; }
    }
}