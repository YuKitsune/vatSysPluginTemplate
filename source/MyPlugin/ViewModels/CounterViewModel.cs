using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MediatR;
using MyPlugin.Core.Contracts;
using vatSysToolbox;

namespace MyPlugin.ViewModels;

public partial class CounterViewModel : ObservableObject
{
    readonly IMediator _mediator;
    readonly IErrorReporter _errorReporter;

    [ObservableProperty] int _count;

    public CounterViewModel(IMediator mediator, IErrorReporter errorReporter)
    {
        _mediator = mediator;
        _errorReporter = errorReporter;

        // The window can open after the counter changes. Thus, get the current value.
        _ = LoadCount();
    }

    async Task LoadCount()
    {
        try
        {
            Count = await _mediator.Send(new GetCounterRequest());
        }
        catch (Exception ex)
        {
            _errorReporter.ReportError(ex, "Cannot get the value of the counter");
        }
    }

    // The command runs on the UI thread, and the await continues on the UI thread. Thus, you can change Count here.
    [RelayCommand]
    async Task Increment()
    {
        try
        {
            Count = await _mediator.Send(new IncrementCounterRequest());
        }
        catch (Exception ex)
        {
            _errorReporter.ReportError(ex, "Cannot increment the counter");
        }
    }
}
