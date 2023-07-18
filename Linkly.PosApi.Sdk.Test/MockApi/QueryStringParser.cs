using System.Web;

namespace Linkly.PosApi.Sdk.UnitTest.MockApi;

internal static class QueryStringParser
{
    public static IDictionary<string, string> Parse(string? queryString)
    {
        var query = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        if (queryString is null) return query;

        foreach (var @params in HttpUtility.UrlDecode(queryString.TrimStart('?')).Split('&'))
        {
            var keyValue = @params.Split('=');

            if (keyValue.Length != 2)
                throw new ArgumentException("Not a valid query string", queryString);

            query[keyValue[0]] = keyValue[1];
        }

        return query;
    }
}