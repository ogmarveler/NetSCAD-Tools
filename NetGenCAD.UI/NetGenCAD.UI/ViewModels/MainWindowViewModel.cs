using Avalonia;
using Avalonia.Styling;
using NetGenCAD.UI.Views;
using Microsoft.Extensions.DependencyInjection;
using ReactiveUI;
using System.Diagnostics.CodeAnalysis;
using System.Windows.Input;

namespace NetGenCAD.UI.ViewModels
{
    public class MainWindowViewModel : ReactiveObject
    {
        // Set MainView as the initial content
        private object? _mainViewContent = App.Services!.GetRequiredService<MainView>();

        [RequiresUnreferencedCode("MainWindowViewModel may use code that is not referenced directly and could be trimmed by the linker.")]
        [UnconditionalSuppressMessage("AOT", "IL3050:Calling members annotated with 'RequiresDynamicCodeAttribute' may break functionality when AOT compiling.", Justification = "<Pending>")]
        public MainWindowViewModel()
        {
            MainViewContent = _mainViewContent; // Start with this view
            // Initialize menu commands
            NewAxesCommand = ReactiveCommand.Create(LoadCreateAxesView);
            NewObjectCommand = ReactiveCommand.Create(LoadScadObjectView);
            NewShapeCommand = ReactiveCommand.Create(LoadScadShapeView);
            OpenFolderCommand = ReactiveCommand.CreateFromTask(MainWindow.OpenFolderAsync);
            ToggleCommand = ReactiveCommand.Create(ToggleTheme);
            AxisViewCommand = ReactiveCommand.Create(LoadAxisView);
            DesignerViewCommand = ReactiveCommand.Create(LoadDesignerView);
            ShapeViewCommand = ReactiveCommand.Create(LoadShapeView);
            MainViewCommand = ReactiveCommand.Create(LoadMainView);
        }

        public object? MainViewContent { get => _mainViewContent; set => this.RaiseAndSetIfChanged(ref _mainViewContent, value); }
        public static void ToggleTheme() => Application.Current?.RequestedThemeVariant = Application.Current.ActualThemeVariant == ThemeVariant.Light ? ThemeVariant.Dark : ThemeVariant.Light;

        // SPA - Swap out views
        public void LoadCreateAxesView() => MainViewContent = App.Services!.GetRequiredService<CreateAxesView>();
        public void LoadAxisView() => MainViewContent = App.Services!.GetRequiredService<AxisView>();
        public void LoadDesignerView() => MainViewContent = App.Services!.GetRequiredService<DesignerView>();
        public void LoadShapeView() => MainViewContent = App.Services!.GetRequiredService<ShapeView>();
        public void LoadMainView() => MainViewContent = App.Services!.GetRequiredService<MainView>();
        public void LoadScadShapeView()
        {
            App.Services!.GetRequiredService<ScadShapeViewModel>().GetAxesList();  // Refresh Axes List if using singleton or scoped services
            MainViewContent = App.Services!.GetRequiredService<ScadShapeView>();
        }
        public void LoadScadObjectView()
        {
            App.Services!.GetRequiredService<ScadObjectViewModel>().GetAxesList();  // Refresh Axes List if using singleton or scoped services
            App.Services!.GetRequiredService<ScadObjectViewModel>().GetPolyhedronsList(); // Refresh AvailablePolyhedrons List if using singleton
            MainViewContent = App.Services!.GetRequiredService<ScadObjectView>();
        }

        // Menu Commands
        public ICommand NewAxesCommand { get; }
        public ICommand NewObjectCommand { get; }
        public ICommand NewShapeCommand { get; }
        public ICommand OpenFolderCommand { get; }
        public ICommand AxisViewCommand { get; }
        public ICommand DesignerViewCommand { get; }
        public ICommand ShapeViewCommand { get; }
        public ICommand MainViewCommand { get; }
        public ICommand ToggleCommand { get; }
    }
}