using Avalonia.Controls;
using Avalonia.Interactivity;
using Microsoft.Extensions.DependencyInjection;
using NetScad.UI.ViewModels;
using System.Threading.Tasks;

namespace NetScad.UI.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = App.Host.Services.GetRequiredService<MainWindowViewModel>();
        }

        private async void OpenFolderPickerAsync(object? sender, RoutedEventArgs e)
        {
            var folderPickerDataContext = new FolderPickerViewModel(this);
            await folderPickerDataContext.OpenFolderPickerAsync();
        }

        public async Task OpenFolderAsync()
        {
            var folderPickerDataContext = new FolderPickerViewModel(this);
            await folderPickerDataContext.OpenFolderAsync();
        }
    }
}