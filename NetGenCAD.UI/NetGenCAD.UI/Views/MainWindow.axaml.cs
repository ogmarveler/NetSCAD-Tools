using Avalonia.Controls;
using NetGenCAD.Core.Interfaces;
using NetGenCAD.Designer.Utility;
using NetGenCAD.UI.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;

namespace NetGenCAD.UI.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            this.WindowState = WindowState.Maximized;
            DataContext = App.Services!.GetRequiredService<MainWindowViewModel>();
        }

        public static async Task OpenFolderAsync()
        {
            var scadPath = App.Services!.GetRequiredService<IScadPathProvider>().ScadPath;

            await ScadFileOperations.OpenFolderAsync(scadPath);
        }
    }
}