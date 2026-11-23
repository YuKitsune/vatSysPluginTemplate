namespace MyPlugin.Core;

/// <summary>
/// An example of state that the plugin keeps. Replace it with the state of your plugin.
/// </summary>
/// <remarks>
/// The MediatR handlers can run on any thread. Thus, the counter is thread-safe.
/// </remarks>
public class Counter
{
    int _value;

    public int Value => Volatile.Read(ref _value);

    public int Increment() => Interlocked.Increment(ref _value);
}
