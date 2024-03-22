using System;
using AnsiRenderer;
namespace HeatManagement;

static partial class CLI
{
    private static readonly Renderer renderer = new();

    public static void Run(Arguments arguments)
    {
        Console.CursorVisible = false;
        Console.CancelKeyPress += delegate
        {
            Console.SetCursorPosition(0, 0);
            Console.ResetColor();
            Console.Clear();
            Console.CursorVisible = true;
        };
        RunGreeter(arguments);
        Console.CursorVisible = true;
    }
}