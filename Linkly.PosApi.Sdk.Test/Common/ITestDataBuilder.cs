namespace Linkly.PosApi.Sdk.UnitTest.Common;

internal interface ITestDataBuilder : IEnumerable<object[]>
{
    IEnumerable<(object TestData, string PropertyName)> GetTests();
}