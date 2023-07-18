using Linkly.PosApi.Sdk.Models.SendKey;
using Linkly.PosApi.Sdk.UnitTest.Common;

namespace Linkly.PosApi.Sdk.UnitTest.TestData;

internal sealed class SendKeyRequestNegativeTestDataBuilder : TestDataBuilder<SendKeyRequest>
{
    public SendKeyRequestNegativeTestDataBuilder() : base(() => RequestRepository.GetSendKeyRequest(Guid.NewGuid()))
    {
        Include<PosApiRequestNegativeTestDataBuilder>();

        Add(req => req.SessionId, Guid.Empty);
        Add(req => req.Key, null);
        Add(req => req.Key, "");
        Add(req => req.Data, new string('0', 61));
    }
}