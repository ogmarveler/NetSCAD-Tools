using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using System;
using System.ComponentModel;

namespace NetScad.UI.Views;

public partial class ModalDialog : UserControl, INotifyPropertyChanged
{
    public static readonly StyledProperty<bool> IsOpenProperty =
        AvaloniaProperty.Register<ModalDialog, bool>(nameof(IsOpen), defaultValue: false);

    public static readonly StyledProperty<string> TitleProperty =
        AvaloniaProperty.Register<ModalDialog, string>(nameof(Title), defaultValue: "");

    public static readonly StyledProperty<object?> ContentProperty =
        AvaloniaProperty.Register<ModalDialog, object?>(nameof(Content));

    public static readonly StyledProperty<object?> FooterProperty =
        AvaloniaProperty.Register<ModalDialog, object?>(nameof(Footer));

    public static readonly StyledProperty<bool> ShowFooterProperty =
        AvaloniaProperty.Register<ModalDialog, bool>(nameof(ShowFooter), defaultValue: false);

    public bool IsOpen
    {
        get => GetValue(IsOpenProperty);
        set => SetValue(IsOpenProperty, value);
    }

    public string Title
    {
        get => GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public object? Content
    {
        get => GetValue(ContentProperty);
        set => SetValue(ContentProperty, value);
    }

    public object? Footer
    {
        get => GetValue(FooterProperty);
        set => SetValue(FooterProperty, value);
    }

    public bool ShowFooter
    {
        get => GetValue(ShowFooterProperty);
        set => SetValue(ShowFooterProperty, value);
    }

    public event EventHandler? CloseRequested;

    public ModalDialog()
    {
        InitializeComponent();
    }

    private void CloseButton_Click(object? sender, RoutedEventArgs e)
    {
        IsOpen = false;
        CloseRequested?.Invoke(this, EventArgs.Empty);
    }
}