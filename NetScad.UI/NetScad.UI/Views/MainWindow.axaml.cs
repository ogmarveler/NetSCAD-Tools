using Avalonia.Controls;
using Avalonia.Interactivity;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NetScad.UI.ViewModels;
using System;
using System.Data.Common;
using System.Threading.Tasks;

namespace NetScad.UI.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
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