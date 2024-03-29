using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;

namespace HeatManagement.GUI;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public static TopLevel TopLevel { get; private set; } = null!; //pretend this is a late initialization

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow
            {
                DataContext = new GreeterViewModel(new(desktop.Args!))
            };
            TopLevel = TopLevel.GetTopLevel(desktop.MainWindow)!;
        }
        else if (ApplicationLifetime is ISingleViewApplicationLifetime singleViewPlatform)
        {
            singleViewPlatform.MainView = new MainWindow
            {
                DataContext = new GreeterViewModel(new([]))
            };
            TopLevel = TopLevel.GetTopLevel(singleViewPlatform.MainView)!;
        }

        base.OnFrameworkInitializationCompleted();
    }
}