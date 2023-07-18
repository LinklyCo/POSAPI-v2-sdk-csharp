using System;
using System.Linq;
using Linkly.PosApi.Sdk.Common;

namespace Linkly.PosApi.Sdk.Models.Transaction
{
    /// <summary>
    /// Provides up to three tipping options to present to the customer as a percentage of the
    /// total purchase amount.
    /// </summary>
    public class TippingOptions : IPurchaseAnalysisDataConverter
    {
        /// <summary>
        /// First tipping option percentages to display to the customer. Minimum of one and maximum of
        /// three.
        /// </summary>
        private readonly byte[] _tipPercentages;

        /// <summary></summary>
        /// <param name="tipPercentages">Provide at least one, and up to three tipping percentage options.</param>
        public TippingOptions(params byte[] tipPercentages)
        {
            if (tipPercentages.Length < 1)
                throw new ArgumentException("Must provide at least one tipping percentages", nameof(tipPercentages));
            if (tipPercentages.Length > 3)
                throw new ArgumentException("Must provide at most three tipping percentages", nameof(tipPercentages));

            foreach (var percentage in tipPercentages)
            {
                if (percentage < 1)
                    throw new ArgumentException("Tipping percentage must be greater than 1.", nameof(tipPercentages));
                if (percentage > 99)
                    throw new ArgumentException("Tipping percentage must be less than 99.", nameof(tipPercentages));
            }

            _tipPercentages = tipPercentages;
        }

        string IPurchaseAnalysisDataConverter.ToPadString() =>
            "[" + string.Join(",", _tipPercentages.Select(p => p.ToString("D2"))) + "]";
    }
}