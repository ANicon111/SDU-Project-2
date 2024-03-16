using System;
using System.IO;
using System.Threading;
using AnsiRenderer;
namespace HeatManagement;

static partial class CLI
{
    public static void RunGreeter(string[] args)
    {
        Console.CursorVisible = false;
        AssetManager? assets = null;
        string tryLoadAssetsFile(string filePath)
        {
            if (!File.Exists(filePath))
                return "Assets file path is invalid";
            else
                try
                {
                    assets = new(File.ReadAllText(filePath));
                }
                catch
                {
                    return "Assets file is inaccessible or has invalid json";
                }
            return "";
        }

        FilePathMenu(
            args: args,
            filePath: "assets.json",
            title: "Input the asset file path:",
            tryLoadFile: tryLoadAssetsFile
        );

        SourceDataManager? sourceData = null;
        string tryLoadSourceDataFile(string filePath)
        {
            if (!File.Exists(filePath))
                return "Source data file path is invalid";
            else
                try
                {
                    sourceData = new(File.ReadAllText(filePath));
                }
                catch
                {
                    return "Source data file is inaccessible or has invalid json";
                }
            return "";
        }


        FilePathMenu(
            args: args,
            filePath: "sourceData.json",
            title: "Input the source data file path:",
            tryLoadFile: tryLoadSourceDataFile
        );

        RunGraphList(assets!, sourceData!);
    }
    public static void FilePathMenu(string[] args, string filePath, string title, Func<string, string> tryLoadFile)
    {
        bool fileLoaded = false;
        string error = "";

        if (args.Length > 0)
        {
            filePath = args[0];
            args = args[1..];
            error = tryLoadFile(filePath);
            fileLoaded = error == "";
        }

        int selectedChar = filePath.Length - 1;
        while (!fileLoaded)
        {
            renderer.Object = new(
                geometry: new(0, 0, renderer.TerminalWidth, renderer.TerminalHeight),
                internalAlignmentX: Alignment.Center,
                internalAlignmentY: Alignment.Center,
                defaultCharacter: ' ',
                subObjects: [
                    new(
                        geometry: new(0, 0, renderer.TerminalWidth * 3 / 4, 5),
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
                            new(color:Colors.White, geometry: new(0, 1, renderer.TerminalWidth * 3 / 4 - 4, 1)),
                            new(color:Colors.Black, geometry: new(0, 1, renderer.TerminalWidth * 3 / 4 - 4, 1), foreground:true),
                            new(color:Colors.Black, geometry: new( -filePath.Length / 2 + selectedChar + 1, 1, 1, 1)),
                            new(color:Colors.White, geometry: new( -filePath.Length / 2 + selectedChar + 1, 1, 1, 1), foreground:true),
                            new(color:Colors.Red, geometry: new(0, -1, renderer.TerminalWidth * 3 / 4 - 4, 1), foreground:true),
                        ],
                        border: Borders.Rounded
                    ),
                ]
            );

            while (Console.KeyAvailable)
            {
                ConsoleKeyInfo key = renderer.ReadKey();
                switch (key.Key)
                {
                    case ConsoleKey.Enter:
                        error = tryLoadFile(filePath);
                        fileLoaded = error == "";
                        break;
                    case ConsoleKey.Backspace:
                        if (filePath.Length > 0)
                            filePath = filePath.Remove(selectedChar, 1);
                        selectedChar = Math.Max(selectedChar - 1, 0);
                        break;
                    case ConsoleKey.RightArrow:
                        selectedChar = Math.Min(selectedChar + 1, filePath.Length - 1);
                        break;
                    case ConsoleKey.LeftArrow:
                        selectedChar = Math.Max(selectedChar - 1, 0);
                        break;
                    default:
                        char c = key.KeyChar;
                        if (c > 31)
                        {
                            selectedChar++;
                            filePath = filePath[..selectedChar] + key.KeyChar + filePath[selectedChar..];
                        }
                        break;
                }
            }
            Thread.Sleep(50);
            if (renderer.UpdateScreenSize())
            {
                renderer.Object.Width = renderer.TerminalWidth;
                renderer.Object.Height = renderer.TerminalHeight;
                renderer.Object.SubObjects[0].Width = renderer.TerminalWidth * 3 / 4;
            }
            renderer.Update();
        }
    }
}