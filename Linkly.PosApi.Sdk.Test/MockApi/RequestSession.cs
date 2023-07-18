using System.Collections.Concurrent;
using Linkly.PosApi.Sdk.UnitTest.MockApi.Models;

namespace Linkly.PosApi.Sdk.UnitTest.MockApi;

/// <summary>
/// Store a collection of <see cref="PosApiRequest" /> and corresponding <see cref="PosApiResponse" />
/// objects associated with a session.
/// </summary>
internal class RequestSession
{
    private readonly ConcurrentQueue<IBaseRequest> _requests = new();
    private readonly ConcurrentQueue<IBaseResponse> _responses = new();
    public IReadOnlyCollection<IBaseRequest> Requests => _requests;
    public IReadOnlyCollection<IBaseResponse> Responses => _responses;
    public void AddRequest(IBaseRequest request) => _requests.Enqueue(request);
    public void AddResponse(IBaseResponse response) => _responses.Enqueue(response);
}