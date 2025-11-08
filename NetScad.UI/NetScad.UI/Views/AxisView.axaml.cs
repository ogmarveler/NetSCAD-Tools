using Avalonia.Controls;
using Avalonia.Platform;
using Markdown.Avalonia;
using Microsoft.Extensions.DependencyInjection;
using NetScad.UI.ViewModels;
using ReactiveUI;
using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;

namespace NetScad.UI.Views;

public partial class AxisView : UserControl
{
    [RequiresDynamicCode("AxisView uses APIs that require dynamic code generation.")]
    public AxisView()
    {
        InitializeComponent();
        DataContext = App.Services!.GetRequiredService<AxisViewModel>();
        LoadMarkdownAsync("avares://NetScad.UI/Assets/Guides/Axis.markdown");  // Relative or absolute path
    }

    [UnconditionalSuppressMessage("Trimming", "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code", Justification = "<Pending>")]
    [RequiresDynamicCode("Calls ReactiveUI.ReactiveCommand.Create<TParam>(Action<TParam>, IObservable<Boolean>, IScheduler)")]
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