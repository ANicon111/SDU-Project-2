using System;
using System.Threading;
using AnsiRenderer;

namespace HeatManagement.CLI;

static partial class App
{
    //generic file loader for a manager
    public static void TextBox(ref bool escaped, string text, string title, Func<string, string> fileAction, bool tryInitialAction = false, bool numbersOnly = false)
    {
        bool fileLoaded = false;

        //try to load file
        string error = tryInitialAction ? fileAction(text) : "";
        if (tryInitialAction && error == "") fileLoaded = true;

        int selectedChar = text.Length;

        renderer.Object.SubObjects.Add(new());
        while (!fileLoaded && !escaped)
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
                        {text}
                        """,
                        colorAreas: [
                            new(color:Colors.Black.WithAlpha(0.75)),
                            new(color:Colors.White, geometry: new(0, 1, renderer.TerminalWidth * 3 / 4 - 4, 1)),
                            new(color:Colors.Black, geometry: new(0, 1, renderer.TerminalWidth * 3 / 4 - 4, 1), foreground:true),
                            new(color:Colors.Black, geometry: new( -text.Length / 2 + selectedChar, 1, 1, 1)),
                            new(color:Colors.White, geometry: new( -text.Length / 2 + selectedChar, 1, 1, 1), foreground:true),
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
                        error = fileAction(text);
                        fileLoaded = error == "";
                        break;

                    case ConsoleKey.Backspace:
                        if (text.Length > 0 && selectedChar > 0)
                            text = text.Remove(selectedChar - 1, 1);
                        selectedChar = Math.Max(selectedChar - 1, 0);
                        break;
                    case ConsoleKey.Delete:
                        if (text.Length > 0 && selectedChar < text.Length)
                            text = text.Remove(selectedChar, 1);
                        break;
                    case ConsoleKey.RightArrow:
                        selectedChar = Math.Min(selectedChar + 1, text.Length);
                        break;

                    case ConsoleKey.LeftArrow:
                        selectedChar = Math.Max(selectedChar - 1, 0);
                        break;
                    case ConsoleKey.Escape:
                        escaped = true;
                        break;

                    default:
                        char c = key.KeyChar;
                        //ignore null and control characters, and only allow numbers and ,.: for a number input
                        if (c > 31 && (!numbersOnly || "0123456789,.:-".Contains(c)))
                        {
                            if (text != "")
                            {
                                text = text[..selectedChar] + key.KeyChar + text[selectedChar..];
                                selectedChar++;
                            }
                            else
                            {
                                text = key.KeyChar.ToString();
                                selectedChar++;
                            }
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