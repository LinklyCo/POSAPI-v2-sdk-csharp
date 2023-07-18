using Linkly.PosApi.Sdk.Models;
using Linkly.PosApi.Sdk.Models.Logon;
using Linkly.PosApi.Sdk.UnitTest.Common;

namespace Linkly.PosApi.Sdk.UnitTest.TestData;

internal sealed class LogonRequestNegativeTestDataBuilder : TestDataBuilder<LogonRequest>
{
    public LogonRequestNegativeTestDataBuilder() : base(RequestRepository.GetLogonRequest)
    {
        Include<PosApiRequestNegativeTestDataBuilder>();

        Add(req => req.LogonType, (LogonType)(-1));
        Add(req => req.LogonType, (LogonType)int.MaxValue);
    }
}