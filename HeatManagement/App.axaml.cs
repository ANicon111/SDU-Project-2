using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;

namespace HeatManagement.Views;

public partial class App : Application
{
    public static TopLevel TopLevel{get;private set;}= null!;
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow();
            TopLevel = TopLevel.GetTopLevel(desktop.MainWindow)!;
        }

        base.OnFrameworkInitializationCompleted();
    }

}