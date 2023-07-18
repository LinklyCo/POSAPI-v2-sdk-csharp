using System.Collections;
using System.Collections.Concurrent;
using Linkly.PosApi.Sdk.UnitTest.MockApi.Models;

namespace Linkly.PosApi.Sdk.UnitTest.MockApi;

/// <summary>Store mock requests and responses for a session ID.</summary>
internal class RequestSessionManager : IEnumerable<RequestSession>
{
    private readonly ConcurrentDictionary<Guid, RequestSession> _sessions = new();

    public IEnumerator<RequestSession> GetEnumerator() => _sessions.Values.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    /// <summary>Add a new request to the collection.</summary>
    /// <param name="sessionId">Unique identifier the request belongs to.</param>
    /// <param name="request">Request object.</param>
    /// <returns>True if the request was added. False if it already exists.</returns>
    public void AddRequest(Guid sessionId, IBaseRequest request)
    {
        _sessions.TryAdd(sessionId, new RequestSession());
        _sessions[sessionId].AddRequest(request);
    }

    /// <summary>
    /// Add a response to an existing request session. Must be called after <see cref="AddRequest" /> with
    /// the same <see cref="sessionId" />.
    /// </summary>
    /// <param name="sessionId">Unique identifier the responses belong to.</param>
    /// <param name="response">Response object.</param>
    /// <returns>
    /// true if the response was added or false if it wasn't added because a request does not exist for the
    /// <see cref="sessionId" />.
    /// </returns>
    public bool AddResponse(Guid sessionId, IBaseResponse response)
    {
        if (!_sessions.TryGetValue(sessionId, out var session))
            return false;

        session.AddResponse(response);
        return true;
    }

    /// <summary>Get requests and responses associated with a specific <see cref="sessionId" />.</summary>
    /// <param name="sessionId">Session to retrieve.</param>
    /// <param name="requests">Requests in the session.</param>
    /// <param name="responses">Responses in the session.</param>
    /// <returns>true if the session exists or false otherwise.</returns>
    public bool TryGetSession(Guid sessionId, out ICollection<IBaseRequest> requests, out ICollection<IBaseResponse> responses)
    {
        if (_sessions.TryGetValue(sessionId, out var requestSession))
        {
            requests = requestSession.Requests.ToList();
            responses = requestSession.Responses.ToList();

            return true;
        }

        requests = null!;
        responses = null!;

        return false;
    }
}