namespace HeatManagement;
using Avalonia;
using System;

class Program
{
    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static int Main(string[] args)
    {
        Arguments arguments;
        try
        {
            arguments = new(args);
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
            Console.WriteLine(Arguments.HelpMessage);
            return 1;
        }

        if (arguments.Help)
        {
            Console.WriteLine(Arguments.HelpMessage);
            return 0;
        }

        if (arguments.CLIMode)
        {
            CLI.App.Run(arguments);
            return 0;
        }

        if (arguments.AvaloniaMode)
        {
            BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
            return 0;
        }

        return 1;
    }

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<GUI.App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace();
}
