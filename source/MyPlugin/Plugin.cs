using System.ComponentModel.Composition;
using System.Windows.Forms;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using MyPlugin.Core;
using MyPlugin.ViewModels;
using MyPlugin.Windows;
using vatsys;
using vatsys.Plugin;
using vatSysToolbox;
using vatSysToolbox.Extensions;
using vatSysToolbox.MediatR;
using vatSysToolbox.Messaging;
using vatSysToolbox.Windows;

namespace MyPlugin;

[Export(typeof(IPlugin))]
public class Plugin : IPlugin
{
#if DEBUG
    public const string Name = "My Plugin - Debug";
#else
    public const string Name = "My Plugin";
#endif

    /// <summary>
    /// The name that other plugins use to send messages to this plugin.
    /// </summary>
    public const string MailboxName = "dev.example.my-plugin";

    readonly IErrorReporter _errorReporter;
    readonly WindowManager _windowManager;
    readonly IMediator _mediator;

    string IPlugin.Name => Name;

    public Plugin()
    {
        var serviceProvider = new ServiceCollection()
            .AddLogging()
            .AddErrorReporter(Name)
            .AddClock()
            .AddWindowManager()
            .AddWorkQueue()
            .AddPluginMessenger(MailboxName)
            .AddMediatR(c => c
                .RegisterServicesFromAssemblyContaining<Plugin>()
                .RegisterServicesFromAssemblyContaining<Counter>())
            .AddSingleton<Counter>()
            .BuildServiceProvider();

        _errorReporter = serviceProvider.GetRequiredService<IErrorReporter>();
        _windowManager = serviceProvider.GetRequiredService<WindowManager>();
        _mediator = serviceProvider.GetRequiredService<IMediator>();

        CreateMenuItems();

        // Connect the handlers that have the PluginMessageHandler attribute to the messages from other plugins.
        // Add all the handlers before you start the messenger.
        var messenger = serviceProvider.GetRequiredService<IPluginMessenger>();
        messenger.SubscribeHandlers(typeof(Plugin).Assembly, _mediator, new JsonMessageSerializer());
        messenger.Start();
    }

    void CreateMenuItems()
    {
        var menuItem = new CustomToolStripMenuItem(
            CustomToolStripMenuItemWindowType.Main,
            CustomToolStripMenuItemCategory.Tools,
            new ToolStripMenuItem(Name));
        menuItem.Item.Click += (_, _) => ToggleCounterWindow();

        MMI.AddCustomMenuItem(menuItem);
    }

    // The menu item click occurs on the UI thread. An exception that is not caught on the UI thread stops vatSys.
    void ToggleCounterWindow()
    {
        try
        {
            _windowManager.Toggle(
                WindowKeys.Counter,
                Name,
                _ => new CounterView(new CounterViewModel(_mediator, _errorReporter)),
                new WindowOptions { CanMinimise = false });
        }
        catch (Exception ex)
        {
            _errorReporter.ReportError(ex, "Cannot open or close the window");
        }
    }

    public void OnFDRUpdate(FDP2.FDR updated)
    {
    }

    public void OnRadarTrackUpdate(RDP.RadarTrack updated)
    {
    }
}
