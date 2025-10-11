using Avalonia.Controls;
using NetScad.UI.ViewModels;
using System.ComponentModel;

namespace NetScad.UI.Views;

public partial class ScadObjectView : UserControl, INotifyPropertyChanged
{
    public ScadObjectView()
    {
        InitializeComponent();
        DataContext = new ScadObjectViewModel();
    }
}