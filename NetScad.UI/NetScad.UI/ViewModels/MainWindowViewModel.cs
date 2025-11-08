using Avalonia;
using Avalonia.Styling;
using Microsoft.Extensions.DependencyInjection;
using NetScad.UI.Views;
using ReactiveUI;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using System.Windows.Input;

namespace NetScad.UI.ViewModels
{
    public class MainWindowViewModel : ReactiveObject
    {
        // Set MainView as the initial content
        private object? _mainViewContent = App.Services!.GetRequiredService<ScadObjectView>();

        [RequiresUnreferencedCode("MainWindowViewModel may use code that is not referenced directly and could be trimmed by the linker.")]
        [UnconditionalSuppressMessage("AOT", "IL3050:Calling members annotated with 'RequiresDynamicCodeAttribute' may break functionality when AOT compiling.", Justification = "<Pending>")]
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
        public Task LoadCreateAxesView()
        {
            MainViewContent = App.Services!.GetRequiredService<CreateAxesView>();
            return Task.CompletedTask;
        }

        public Task LoadAxisView()
        {
            MainViewContent = App.Services!.GetRequiredService<AxisView>();
            return Task.CompletedTask;
        }

        public Task LoadDesignerView()
        {
            MainViewContent = App.Services!.GetRequiredService<DesignerView>();
            return Task.CompletedTask;
        }

        public async Task LoadScadObjectView()
        {
            await App.Services!.GetRequiredService<ScadObjectViewModel>().GetAxesList();  // Refresh Axes List if using singleton or scoped services
            MainViewContent = App.Services!.GetRequiredService<ScadObjectView>();
        }

        public static Task ToggleTheme()
        {
            Application.Current?.RequestedThemeVariant =
                   Application.Current.ActualThemeVariant == ThemeVariant.Light
                       ? ThemeVariant.Dark
                       : ThemeVariant.Light;
            return Task.CompletedTask;
        }

        public ICommand NewAxesCommand { get; } 
        public ICommand NewObjectCommand { get; }
        public ICommand OpenFolderCommand { get; }
        public ICommand AxisViewCommand { get; }
        public ICommand DesignerViewCommand { get; }
        public ICommand ToggleCommand { get; }
    }
}