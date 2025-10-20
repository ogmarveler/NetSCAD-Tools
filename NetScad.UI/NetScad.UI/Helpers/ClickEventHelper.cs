using Avalonia;
using Avalonia.Controls;
using System;
using System.Reflection;
using Avalonia.VisualTree;

namespace NetScad.UI.Helpers
{
    //public static class ClickEventHelper
    //{
    //    public static readonly AttachedProperty<string> ClickHandlerNameProperty =
    //        AvaloniaProperty.RegisterAttached<Button, string>("ClickHandlerName", typeof(ClickEventHelper));

    //    public static string GetClickHandlerName(Button button) => button.GetValue(ClickHandlerNameProperty);
    //    public static void SetClickHandlerName(Button button, string value) => button.SetValue(ClickHandlerNameProperty, value);

    //    static ClickEventHelper()
    //    {
    //        ClickHandlerNameProperty.Changed.Subscribe(args =>
    //        {
    //            if (args.Sender is Button button)
    //            {
    //                button.Click -= Button_Click;
    //                if (!string.IsNullOrEmpty(args.NewValue.GetValueOrDefault()))
    //                {
    //                    button.Click += Button_Click;
    //                    button.Tag = args.NewValue.GetValueOrDefault(); // Store handler name
    //                }
    //            }
    //        });
    //    }

    //    private static void Button_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
    //    {
    //        if (sender is Button button && button.Tag is string handlerName)
    //        {
    //            var current = button;
    //            while (current != null && !(current is Window || current is UserControl))
    //            {
    //                current = (Button)current.GetVisualParent(); // Use Avalonia's GetVisualParent
    //            }

    //            if (current != null)
    //            {
    //                var type = current.GetType();
    //                var method = type.GetMethod(handlerName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
    //                if (method != null)
    //                {
    //                    method.Invoke(current, new object[] { sender, e });
    //                }
    //                else
    //                {
    //                    Console.WriteLine($"Click handler '{handlerName}' not found in {type.Name}.");
    //                }
    //            }
    //        }
    //    }
    //}
}
