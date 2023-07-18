using System;
using System.Collections.ObjectModel;
using System.Linq;
using Linkly.PosApi.Sdk.Common;

namespace Linkly.PosApi.Sdk.Models.Transaction
{
    /// <summary>
    /// Provides the necessary information to apply a surcharge to a transaction based on the card
    /// bin.
    /// </summary>
    public class SurchargeOptions : IPurchaseAnalysisDataConverter
    {
        private readonly Collection<SurchargeRule> _surcharges = new Collection<SurchargeRule>();

        /// <summary>Add a surcharge rule for the specified card bin. Supported surcharge rules are:
        /// <list type="bullet">
        /// <item><description><see cref="PercentageSurcharge" /></description></item>
        /// <item><description><see cref="FixedSurcharge" /></description></item>
        /// </list>
        /// </summary>
        public void Add(SurchargeRule surchargeRule) =>
            _surcharges.Add(surchargeRule);

        string IPurchaseAnalysisDataConverter.ToPadString() =>
            "[" + string.Join(",", _surcharges.Select(s => s.ToPadSurchargeString())) + "]";
    }

    public abstract class SurchargeRule
    {
        protected readonly string Bin;

        protected SurchargeRule(string bin)
        {
            if (string.IsNullOrEmpty(bin))
                throw new ArgumentException("Surcharge Card bin is required", nameof(bin));
            if (bin.Length < 2 || bin.Length > 6)
                throw new ArgumentException("Surcharge Card bin should be between 2 and 6 digits", nameof(bin));

            Bin = bin;
        }

        internal abstract string ToPadSurchargeString();
    }

    /// <summary>
    /// Create a percentage surcharge rule to be added to <see cref="SurchargeOptions"/>.
    /// </summary>
    public class PercentageSurcharge : SurchargeRule
    {
        private readonly int _basisPoints;

        /// <summary>
        /// Create a new percentage surcharge rule for use with <see cref="SurchargeOptions.Add"/>.
        /// </summary>
        /// <param name="bin">Card bin which the rule should apply to.</param>
        /// <param name="basisPoints">Surcharge percentage (in basis points) to apply.</param>
        /// <exception cref="ArgumentException"></exception>
        public PercentageSurcharge(string bin, int basisPoints) : base(bin)
        {
            if (basisPoints < 1 || basisPoints > 9999)
                throw new ArgumentException("Surcharge percentage should be between 1 and 9999", nameof(basisPoints));

            _basisPoints = basisPoints;
        }

        internal override string ToPadSurchargeString() =>
            $"{{\"b\":\"{Bin}\",\"v\":{_basisPoints}}}";
    }

    /// <summary>
    /// Create a fixed surcharge rule to be added to <see cref="SurchargeOptions"/>.
    /// </summary>
    public class FixedSurcharge : SurchargeRule
    {
        private readonly int _amountInCents;

        /// <summary>
        /// Create a new fixed surcharge rule for use with <see cref="SurchargeOptions.Add"/>.
        /// </summary>
        /// <param name="bin">Card bin which the rule should apply to.</param>
        /// <param name="amountInCents">Surcharge amount to apply.</param>
        /// <exception cref="ArgumentException"></exception>
        public FixedSurcharge(string bin, int amountInCents) : base(bin)
        {
            if (amountInCents < 1 || amountInCents > Constants.Validation.MaxAmount)
                throw new ArgumentException($"Amount should be between 1 and {Constants.Validation.MaxAmount}", nameof(amountInCents));

            _amountInCents = amountInCents;
        }

        internal override string ToPadSurchargeString() =>
            $"{{\"b\":\"{Bin}\",\"t\":\"$\",\"v\":{_amountInCents}}}";
    }
}