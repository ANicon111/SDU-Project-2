using System;
namespace HeatManagement;

static partial class CLI
{
    public static void PrintHelp()
    {
        Console.WriteLine(
            """
            Syntax:
            heatmanagement [OPTIONS] [ASSETS_FILE] [SOURCE_FILE]

            -h, --help: Show this message
            -c, --cli, --console: Run the CLI visualizer
            -g, --gui, --avalonia: Run the Avalonia visualizer
            """
        );
    }
}