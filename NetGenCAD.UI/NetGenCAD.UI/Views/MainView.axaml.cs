using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Platform;
using NetGenCAD.UI.ViewModels;
using Markdown.Avalonia;
using Microsoft.Extensions.DependencyInjection;
using ReactiveUI;
using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;

namespace NetGenCAD.UI.Views;

public partial class MainView : UserControl
{
    [RequiresUnreferencedCode("MainView constructor uses code that may require unreferenced code.")]
    public MainView()
    {
        InitializeComponent();
        DataContext = App.Services!.GetRequiredService<MainViewModel>();
        LoadMarkdownAsync("avares://NetGenCAD.UI/Assets/Guides/Designer.markdown");  // Relative or absolute path; Object Designer guide as main help
    }

    [RequiresUnreferencedCode("LoadMarkdownAsync uses ReactiveUI.ReactiveCommand.Create which may require unreferenced code.")]
    [UnconditionalSuppressMessage("AOT", "IL3050:Calling members annotated with 'RequiresDynamicCodeAttribute' may break functionality when AOT compiling.", Justification = "<Pending>")]
    private async void LoadMarkdownAsync(string avaPath)
    {
        try
        {
            using var stream = AssetLoader.Open(new Uri(avaPath));
            using var reader = new StreamReader(stream);
            var markdownContent = await reader.ReadToEndAsync();
            if (MarkdownView is MarkdownScrollViewer viewer)
            {
                viewer.Markdown = markdownContent; // Now markdownContent is a string
                viewer.Plugins.HyperlinkCommand = ReactiveCommand.Create<string>(url =>
                {
                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = url,
                        UseShellExecute = true
                    });
                });
            }
        }
        catch (Exception ex)
        {
            // Fallback: Display error in the viewer
            if (MarkdownView is MarkdownScrollViewer viewer)
            {
                viewer.Markdown = $"**Error loading Markdown file:** {ex.Message}";
            }
        }
    }
}