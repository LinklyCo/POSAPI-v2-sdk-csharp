using System.Text.RegularExpressions;

namespace Linkly.PosApi.Sdk.UnitTest.MockApi;

/// <summary>
/// Store a mapping of request URLs (defined as regular expressions) to a
/// <typeparamref name="TRequestHandler" />.
/// </summary>
internal class RequestMapper<TRequestHandler> where TRequestHandler : class
{
    private readonly Dictionary<Regex, TRequestHandler> _handlers;

    /// <summary>Instantiate object with routes.</summary>
    /// <param name="handlers">
    /// IEnumerable of <see cref="KeyValuePair" /> where the first element in the pair is a regular
    /// expression string to match the path of a URI and the second element is a request handler to use
    /// when the expression is matched. If named groups are defined in the regular expression the values of
    /// the match will be returned as an output parameter in <see cref="TryGetHandler" />.
    /// <example>
    /// Example (C#):
    /// <code>
    /// var mapper = new RequestMapper(new KeyValuePair&lt;string, RequestHandler&gt;[]
    /// {
    ///     new ("^/v1/pairing/cloudpos", HandlePairing),
    ///     new ("^/v1/tokens/cloudpos", HandleAuth),
    ///     new ("^/v2/sessions/(?&lt;SessionId&gt;.*)/transaction", HandleTransaction)
    /// });
    /// </code>
    /// </example>
    /// </param>
    public RequestMapper(IEnumerable<KeyValuePair<string, TRequestHandler>> handlers)
    {
        _handlers = new Dictionary<Regex, TRequestHandler>();

        foreach (var (regex, handler) in handlers)
            _handlers.Add(
                new Regex(regex, RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.ExplicitCapture),
                handler);
    }

    /// <summary>
    /// Find a request handler which matches the <see cref="uri" /> by searching the handlers passed into
    /// the constructor in FIFO order. If no handlers are matched this method returns false and
    /// <see cref="requestHandler" /> and <see cref="requestParams" /> will be undefined. If a match is
    /// found this method returns true, <see cref="requestHandler" /> will be set to the matching handler
    /// and <see cref="requestParams" /> will be set to an IDictionary mapping of matching group
    /// names-values in the regex.
    /// <example>
    /// Example: If the following regular expression is matched.
    /// <code>
    /// ^/v2/sessions/(?&lt;SessionId&gt;.*)/transaction
    /// </code>
    /// By the URI: <code>https://api.com/v2/sessions/123/transaction</code> Then
    /// <see cref="requestParams" /> will contain the key and value: <code>["SessionId"] = "123"</code>
    /// </example>
    /// </summary>
    /// <param name="uri">URI to match.</param>
    /// <param name="requestHandler">
    /// Matching request handler. Will only be defined if this method returns
    /// true
    /// </param>
    /// <param name="requestParams">
    /// Case-insensitive IDictionary of regular expression group names and corresponding values matched in
    /// the URI. Will only be defined if this method returns true.
    /// </param>
    /// <returns>true if a handler was matched or false otherwise.</returns>
    public bool TryGetHandler(Uri uri, out TRequestHandler requestHandler,
        out IDictionary<string, string> requestParams)
    {
        foreach (var (matcher, handler) in _handlers)
        {
            var match = matcher.Match(uri.AbsolutePath);
            if (!match.Success) continue;

            requestHandler = handler;
            requestParams = match.Groups.Values.Skip(1).ToDictionary(group => group.Name, group => group.Value,
                StringComparer.OrdinalIgnoreCase);

            return true;
        }

        requestHandler = null!;
        requestParams = null!;
        return false;
    }
}