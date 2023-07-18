namespace Linkly.PosApi.Sdk.Models
{
    /// <summary>Base interface implemented by all API requests.</summary>
    public interface IBaseRequest
    {
        object ToDto() => this;
    }
}