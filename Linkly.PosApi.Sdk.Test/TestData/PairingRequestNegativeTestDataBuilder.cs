using Linkly.PosApi.Sdk.Models.Authentication;
using Linkly.PosApi.Sdk.UnitTest.Common;

namespace Linkly.PosApi.Sdk.UnitTest.TestData;

internal sealed class PairingRequestNegativeTestDataBuilder : TestDataBuilder<PairingRequest>
{
    public PairingRequestNegativeTestDataBuilder() : base(RequestRepository.GetPairingRequest)
    {
        Add(req => req.PairCode, null!);
        Add(req => req.PairCode, "");
        Add(req => req.PairCode, " ");
        Add(req => req.Password, null!);
        Add(req => req.Password, "");
        Add(req => req.Password, " ");
        Add(req => req.Username, null!);
        Add(req => req.Username, "");
        Add(req => req.Username, " ");
    }
}