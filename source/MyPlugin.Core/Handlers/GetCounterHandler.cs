using MediatR;
using MyPlugin.Core.Contracts;

namespace MyPlugin.Core.Handlers;

public class GetCounterHandler(Counter counter) : IRequestHandler<GetCounterRequest, int>
{
    public Task<int> Handle(GetCounterRequest request, CancellationToken cancellationToken) =>
        Task.FromResult(counter.Value);
}
