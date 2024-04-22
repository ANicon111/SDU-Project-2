using Avalonia.Controls;
using HeatManagement.ViewModels;

namespace HeatManagement.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        DataContext = new MainWindowViewModel();
        InitializeComponent();
    }

    
}