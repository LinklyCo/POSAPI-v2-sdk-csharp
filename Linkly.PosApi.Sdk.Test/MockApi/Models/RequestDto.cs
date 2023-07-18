namespace Linkly.PosApi.Sdk.UnitTest.MockApi.Models;

internal class RequestDto<TRequest>
{
    public TRequest Request { get; set; } = default!;
}