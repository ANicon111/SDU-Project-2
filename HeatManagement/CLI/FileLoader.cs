using System;
using System.Threading;
using AnsiRenderer;

namespace HeatManagement;

static partial class CLI
{
    //generic file loader for a manager
    public static void FilePathMenu(string filePath, string title, Func<string, string> fileAction, bool tryInitialAction = false)
    {
        bool fileLoaded = false;

        //try to load file
        string error = tryInitialAction ? fileAction(filePath) : "";
        if (tryInitialAction && error == "") fileLoaded = true;

        int selectedChar = filePath.Length;

        renderer.Object.SubObjects.Add(new());
        while (!fileLoaded)
        {
            renderer.Object.SubObjects[^1] =
                    new(
                        geometry: new(0, 0, renderer.TerminalWidth * 3 / 4, 5),
                        externalAlignmentX: Alignment.Center,
                        externalAlignmentY: Alignment.Center,
                        internalAlignmentX: Alignment.Center,
                        internalAlignmentY: Alignment.Center,
                        defaultCharacter: ' ',
                        text:
                        $"""
                        {error}
                        {title}
                        {filePath}
                        """,
                        colorAreas: [
                            new(color:Colors.Black.WithAlpha(0.75)),
                            new(color:Colors.White, geometry: new(0, 1, renderer.TerminalWidth * 3 / 4 - 4, 1)),
                            new(color:Colors.Black, geometry: new(0, 1, renderer.TerminalWidth * 3 / 4 - 4, 1), foreground:true),
                            new(color:Colors.Black, geometry: new( -filePath.Length / 2 + selectedChar, 1, 1, 1)),
                            new(color:Colors.White, geometry: new( -filePath.Length / 2 + selectedChar, 1, 1, 1), foreground:true),
                            new(color:Colors.Red, geometry: new(0, -1, renderer.TerminalWidth * 3 / 4 - 4, 1), foreground:true),
                        ],
                        border: Borders.Rounded
                    );

            while (Console.KeyAvailable)
            {
                ConsoleKeyInfo key = renderer.ReadKey();
                switch (key.Key)
                {
                    case ConsoleKey.Enter:
                        error = fileAction(filePath);
                        fileLoaded = error == "";
                        break;

                    case ConsoleKey.Backspace:
                        if (filePath.Length > 0 && selectedChar > 0)
                            filePath = filePath.Remove(selectedChar - 1, 1);
                        selectedChar = Math.Max(selectedChar - 1, 0);
                        break;
                    case ConsoleKey.Delete:
                        if (filePath.Length > 0)
                            filePath = filePath.Remove(selectedChar, 1);
                        break;

                    case ConsoleKey.RightArrow:
                        selectedChar = Math.Min(selectedChar + 1, filePath.Length);
                        break;

                    case ConsoleKey.LeftArrow:
                        selectedChar = Math.Max(selectedChar - 1, 0);
                        break;

                    default:
                        char c = key.KeyChar;
                        //ignore null and control characters
                        if (c > 31)
                        {
                            if (filePath != "")
                            {
                                filePath = filePath[..selectedChar] + key.KeyChar + filePath[selectedChar..];
                                selectedChar++;
                            }
                            else
                                filePath = key.KeyChar.ToString();
                        }
                        break;
                }
            }

            Thread.Sleep(50);
            if (renderer.UpdateScreenSize())
            {
                renderer.Object.Width = renderer.TerminalWidth;
                renderer.Object.Height = renderer.TerminalHeight;
                renderer.Object.SubObjects[^1].Width = renderer.TerminalWidth * 3 / 4;
            }
            renderer.Update();
        }
        renderer.Object.SubObjects.RemoveAt(renderer.Object.SubObjects.Count - 1);
    }
}