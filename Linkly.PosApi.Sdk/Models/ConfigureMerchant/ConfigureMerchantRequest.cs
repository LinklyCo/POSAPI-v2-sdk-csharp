using FluentValidation;
using FluentValidation.Results;

namespace Linkly.PosApi.Sdk.Models.ConfigureMerchant
{
    /// <summary>Configure a merchant's PIN pad settings.</summary>
    public class ConfigureMerchantRequest : PosApiRequest
    {
        /// <summary>The terminal ID (CatID) to configure the terminal with.</summary>
        /// <example>12345678</example>
        public string CatId { get; set; } = null!;

        /// <summary>The merchant ID (CaID) to configure the terminal with</summary>
        /// <example>0123456789ABCDEF</example>
        public string CaId { get; set; } = null!;

        public override ValidationResult Validate() => new ConfigureMerchantRequestValidator().Validate(this);
    }

    internal class ConfigureMerchantRequestValidator : AbstractValidator<ConfigureMerchantRequest>
    {
        public ConfigureMerchantRequestValidator()
        {
            Include(new PosApiRequestValidator());

            RuleFor(req => req.CatId).NotEmpty();
            RuleFor(req => req.CaId).NotEmpty();
        }
    }
}