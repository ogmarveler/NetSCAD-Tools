using Avalonia.Controls;
using Microsoft.Extensions.DependencyInjection;
using NetScad.Core.Interfaces;
using NetScad.Designer.Utility;
using NetScad.UI.ViewModels;
using System.Threading.Tasks;

namespace NetScad.UI.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            this.WindowState = WindowState.Maximized;
            DataContext = App.Host?.Services.GetRequiredService<MainWindowViewModel>();
        }

        public static async Task OpenFolderAsync()
        {
            var scadPath = App.Host!.Services.GetRequiredService<IScadPathProvider>().ScadPath;

            await ScadFileOperations.OpenFolderAsync(scadPath);
        }
    }
}