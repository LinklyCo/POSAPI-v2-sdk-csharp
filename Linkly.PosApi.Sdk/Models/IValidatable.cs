using FluentValidation.Results;

namespace Linkly.PosApi.Sdk.Models
{
    /// <summary>
    /// Requires an implementer to provide a method to validate itself using
    /// <see cref="FluentValidation" />.
    /// </summary>
    public interface IValidatable
    {
        ValidationResult Validate();
    }
}