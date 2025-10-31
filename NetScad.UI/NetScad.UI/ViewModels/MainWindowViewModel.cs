using Avalonia;
using Avalonia.Styling;
using Microsoft.Extensions.DependencyInjection;
using NetScad.UI.Views;
using ReactiveUI;
using System.Threading.Tasks;
using System.Windows.Input;

namespace NetScad.UI.ViewModels
{
    public class MainWindowViewModel : ReactiveObject
    {
        // Set MainView as the initial content
        private object? _mainViewContent = App.Host?.Services.GetRequiredService<ScadObjectView>();

        public MainWindowViewModel()
        {
            MainViewContent = _mainViewContent; // Start with this view
            // Initialize menu commands
            NewAxesCommand = ReactiveCommand.Create(LoadCreateAxesView);
            NewObjectCommand = ReactiveCommand.Create(LoadScadObjectView);
            OpenFolderCommand = ReactiveCommand.CreateFromTask(MainWindow.OpenFolderAsync);
            ToggleCommand = ReactiveCommand.Create(ToggleTheme);
            AxisViewCommand = ReactiveCommand.Create(LoadAxisView);
            DesignerViewCommand = ReactiveCommand.Create(LoadDesignerView);
        }

        public object? MainViewContent
        {
            get => _mainViewContent;
            set => this.RaiseAndSetIfChanged(ref _mainViewContent, value);
        }

        // SPA - Swap out views
        public async Task LoadCreateAxesView() => MainViewContent = App.Host?.Services.GetRequiredService<CreateAxesView>();
        public async Task LoadAxisView() => MainViewContent = App.Host?.Services.GetRequiredService<AxisView>();
        public async Task LoadDesignerView() => MainViewContent = App.Host?.Services.GetRequiredService<DesignerView>();
        public async Task LoadScadObjectView()
        {
            await App.Host!.Services.GetRequiredService<ScadObjectViewModel>().GetAxesList();  // Refresh Axes List if using singleton or scoped services
            MainViewContent = App.Host!.Services.GetRequiredService<ScadObjectView>();
        }

        public static async Task ToggleTheme()
        {
            Application.Current?.RequestedThemeVariant =
                   Application.Current.ActualThemeVariant == ThemeVariant.Light
                       ? ThemeVariant.Dark
                       : ThemeVariant.Light;
        }

        public ICommand NewAxesCommand { get; } 
        public ICommand NewObjectCommand { get; }
        public ICommand OpenFolderCommand { get; }
        public ICommand AxisViewCommand { get; }
        public ICommand DesignerViewCommand { get; }
        public ICommand ToggleCommand { get; }
    }
}