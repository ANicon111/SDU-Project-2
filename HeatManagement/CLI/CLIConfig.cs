using System;
using AnsiRenderer;
namespace HeatManagement;

static partial class CLI
{
    private static readonly Renderer renderer = new();

    public static void Run(string[] args)
    {
        Console.CursorVisible = false;
        RunGreeter(ref args);
    }
}