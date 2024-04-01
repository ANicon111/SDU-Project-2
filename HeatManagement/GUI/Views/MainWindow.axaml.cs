using Avalonia.Controls;

namespace HeatManagement.GUI;

public partial class MainWindow : Window
{
    private static double fontSize;
    public static new double FontSize { get => fontSize; }
    public MainWindow()
    {
        InitializeComponent();
        fontSize = base.FontSize;
    }
}