using Avalonia.Controls;
using Avalonia.Controls.Templates;
using NetScad.UI.ViewModels;
using System;
using System.Diagnostics.CodeAnalysis;

namespace NetScad.UI
{
    public class ViewLocator : IDataTemplate
    {
        [UnconditionalSuppressMessage("Trimming", "IL2057:UnrecognizedValue", 
            Justification = "View types are guaranteed to exist at runtime as they're part of the compiled application")]
        [UnconditionalSuppressMessage("ReflectionAnalysis", "IL2072:UnrecognizedReflectionPattern",
            Justification = "View instantiation is guaranteed as all view types are included in the application")]
        public Control? Build(object? param)
        {
            if (param is null)
                return null;

            var name = param.GetType().FullName!.Replace("ViewModel", "View", StringComparison.Ordinal);
            var type = Type.GetType(name);

            if (type != null)
            {
                var instance = Activator.CreateInstance(type);
                if (instance is Control control)
                    return control;
            }

            return new TextBlock { Text = "Not Found: " + name };
        }

        public bool Match(object? data)
        {
            return data is ValidatableBase;
        }
    }
}
