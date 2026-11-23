using System.Windows.Controls;
using MyPlugin.ViewModels;

namespace MyPlugin.Windows;

public partial class CounterView : UserControl
{
    public CounterView(CounterViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
