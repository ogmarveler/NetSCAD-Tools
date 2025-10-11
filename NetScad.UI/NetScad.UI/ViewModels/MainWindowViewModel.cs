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
        private object _mainViewContent = App.Host.Services.GetRequiredService<AxisView>();

        public MainWindowViewModel()
        {
            MainViewContent = _mainViewContent; // Start with this view
            // Initialize menu commands
            NewAxesCommand = ReactiveCommand.Create(LoadCreateAxesView);
            NewObjectCommand = ReactiveCommand.Create(LoadDesignerView);
            OpenCommand = ReactiveCommand.Create(() => { using Task _ = new MainWindow().OpenFolderAsync(); });
            ToggleCommand = ReactiveCommand.Create(ToggleTheme);
            AxisViewCommand = ReactiveCommand.Create(LoadAxisView);
        }

        public object MainViewContent
        {
            get => _mainViewContent;
            set => this.RaiseAndSetIfChanged(ref _mainViewContent, value);
        }

        // SPA - Swap out views
        public async void LoadCreateAxesView() => MainViewContent = App.Host.Services.GetRequiredService<CreateAxesView>();
        public async void LoadAxisView() => MainViewContent = App.Host.Services.GetRequiredService<AxisView>();
        public async void LoadDesignerView() => MainViewContent = App.Host.Services.GetRequiredService<ScadObjectView>();
        public async void ToggleTheme() => Application.Current?.RequestedThemeVariant =
                Application.Current.ActualThemeVariant == ThemeVariant.Light
                    ? ThemeVariant.Dark
                    : ThemeVariant.Light;

        public ICommand NewAxesCommand { get; }
        public ICommand NewObjectCommand { get; }
        public ICommand OpenCommand { get; }
        public ICommand AxisViewCommand { get; }
        public ICommand ToggleCommand { get; }
    }
}