using MediatR;
using MyPlugin.Core.Contracts;

namespace MyPlugin.Core.Handlers;

public class IncrementCounterHandler(Counter counter) : IRequestHandler<IncrementCounterRequest, int>
{
    public Task<int> Handle(IncrementCounterRequest request, CancellationToken cancellationToken) =>
        Task.FromResult(counter.Increment());
}
