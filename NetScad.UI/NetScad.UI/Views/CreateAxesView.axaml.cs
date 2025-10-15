using Avalonia.Controls;
using Avalonia.Interactivity;
using Microsoft.Extensions.DependencyInjection;
using NetScad.UI.ViewModels;
using System.ComponentModel;

namespace NetScad.UI.Views;

public partial class CreateAxesView : UserControl, INotifyPropertyChanged
{
    private CreateAxesViewModel ViewModel => (CreateAxesViewModel)DataContext!;
    public CreateAxesView()
    {
        InitializeComponent();
        DataContext = App.Host.Services.GetRequiredService<CreateAxesViewModel>();
    }

    // Convert from one unit to another
    private async void _ConvertInputsAsync(object? sender, RoutedEventArgs e)
    {
        await ViewModel.ConvertInputs(ViewModel._decimalPlaces);
    }
    private async void _CreateCustomAxisAsync(object? sender, RoutedEventArgs e)
    {
        await ViewModel.CreateCustomAxisAsync();
    }
    private async void _ClearInputsAsync(object? sender, RoutedEventArgs e)
    {
        await ViewModel.ClearInputs();
    }
}