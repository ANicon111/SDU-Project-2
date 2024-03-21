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
            renderer.Reset();
            Console.Clear();
            Console.CursorVisible = true;
        };
        RunGreeter(arguments);
        Console.CursorVisible = true;
    }
}