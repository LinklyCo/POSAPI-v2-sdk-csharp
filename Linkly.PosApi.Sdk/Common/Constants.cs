using System;

namespace Linkly.PosApi.Sdk.Common
{
    internal static class Constants
    {
        public static readonly TimeSpan RegexTimeout = TimeSpan.FromMilliseconds(100);

        public static class Validation
        {
            /// <summary>Maximum amount the API supports in cents.</summary>
            public const int MaxAmount = 999999999;
        }

        /// <summary>Tag names in the PurchaseAnalysisData dictionary.</summary>
        internal static class PurchaseAnalysisData
        {
            /// <summary>
            /// Transaction reference number to be provided to follow-up transaction requests and returned
            /// in the transaction response for some initial request types, eg: purchase.
            /// </summary>
            public const string RFN = "RFN";

            /// <summary>
            /// Full transaction amount after tipping and surcharges, returned in a transaction response.
            /// </summary>
            public const string Amount = "AMT";

            /// <summary>Surcharge amount (in cents), returned in a transaction response.</summary>
            public const string Surcharge = "SUR";

            /// <summary>Pre-auth index to request, for use in a pre-auth summary request.</summary>
            public const string PAI = "PAI";

            /// <summary>Cardholder tipping options, for use in a transaction request.</summary>
            public const string TPO = "TPO";

            /// <summary>
            /// Tip amount (in cents) to apply on top of the transaction amount, for use in a transaction
            /// request.
            /// </summary>
            public const string Tip = "TIP";

            /// <summary>
            /// Surcharge options to apply based on card bin, for use in a transaction request.
            /// </summary>
            public const string SC2 = "SC2";

            /// <summary>
            /// Indicate whether a restricted item is present in the sale, for use in a transaction request.
            /// </summary>
            public const string PLB = "PLB";
        }

        public static class ResponseType
        {
            public const string Logon = "logon";
            public const string Status = "status";
            public const string Display = "display";
            public const string Receipt = "receipt";
            public const string ConfigureMerchant = "configuremerchant";
            public const string QueryCard = "querycard";
            public const string ReprintReceipt = "reprintreceipt";
            public const string Transaction = "transaction";
            public const string Settlement = "settlement";
        }
    }
}