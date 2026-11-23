using MediatR;

namespace MyPlugin.Core.Contracts;

/// <summary>
/// Adds one to the counter. The response is the new value of the counter.
/// </summary>
public record IncrementCounterRequest : IRequest<int>;

/// <summary>
/// Gets the current value of the counter.
/// </summary>
public record GetCounterRequest : IRequest<int>;
