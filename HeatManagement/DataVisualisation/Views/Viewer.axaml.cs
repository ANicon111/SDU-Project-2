using Avalonia.Controls;
using Avalonia.Input;
using HeatManagement.ViewModels;

namespace HeatManagement.Views;

public partial class Viewer : UserControl
{
    public Viewer()
    {
        InitializeComponent();
    }

    public void PointerPressedHandler(object sender, PointerPressedEventArgs args)
    {
        ((ViewerViewModel)DataContext!).SelectGraphPositionPointer(sender, args);
    }
}