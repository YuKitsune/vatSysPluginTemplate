using MyPlugin.Core;
using MyPlugin.Core.Contracts;
using MyPlugin.Core.Handlers;
using Shouldly;

namespace MyPlugin.Tests;

public class CounterTests
{
    [Fact]
    public async Task IncrementCounter_ReturnsTheNewValue()
    {
        var counter = new Counter();
        var handler = new IncrementCounterHandler(counter);

        var first = await handler.Handle(new IncrementCounterRequest(), CancellationToken.None);
        var second = await handler.Handle(new IncrementCounterRequest(), CancellationToken.None);

        first.ShouldBe(1);
        second.ShouldBe(2);
    }

    [Fact]
    public async Task GetCounter_ReturnsTheCurrentValue()
    {
        var counter = new Counter();
        counter.Increment();
        counter.Increment();
        var handler = new GetCounterHandler(counter);

        var value = await handler.Handle(new GetCounterRequest(), CancellationToken.None);

        value.ShouldBe(2);
    }

    [Fact]
    public async Task Increment_FromManyThreads_CountsEachIncrement()
    {
        var counter = new Counter();

        await Task.WhenAll(Enumerable.Range(0, 1000).Select(_ => Task.Run(counter.Increment)));

        counter.Value.ShouldBe(1000);
    }
}
