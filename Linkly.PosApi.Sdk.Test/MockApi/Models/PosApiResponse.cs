using Newtonsoft.Json;

namespace Linkly.PosApi.Sdk.UnitTest.MockApi.Models;

internal class PosApiResponse : IBaseResponse
{
    [JsonIgnore] public bool Consumed { get; set; }

    public string ResponseType { get; set; } = null!;
}