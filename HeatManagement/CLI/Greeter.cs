using System;
using System.Threading;
using AnsiRenderer;
namespace HeatManagement.CLI;

static partial class App
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

        //prompt the user on weather to enter edit mode
        string? mode = null;
        if (arguments.EditPath == null && arguments.AssetsPath == null && arguments.DataPath == null)
        {
            ModeSelector(ref mode);
        }

        if (arguments.EditPath != null || mode == "edit")
            RunEditorGreeter(arguments);
        else
            RunViewerGreeter(arguments);
        Console.CursorVisible = true;
    }

    private static void ModeSelector(ref string? mode)
    {
        renderer.Object = new(
            defaultCharacter: ' ',
            geometry: new(0, 0, renderer.TerminalWidth, renderer.TerminalHeight),
            internalAlignmentX: Alignment.Center,
            internalAlignmentY: Alignment.Center,
            subObjects: [
                new(
                        text:
                        """

                              ╻   
                            ╻ ┃   
                          ┃╻┃┃┃┃  
                          Viewer  
                        
                        """,
                        defaultCharacter: ' ',
                        colorAreas:[
                            new(Colors.Green, geometry: new(3,2,1,3), foreground:true),
                            new(Colors.Yellow, geometry: new(4,2,1,3), foreground:true),
                            new(Colors.Green, geometry: new(5,2,1,3), foreground:true),
                            new(Colors.Yellow, geometry: new(6,2,1,3), foreground:true),
                            new(Colors.Green, geometry: new(7,2,1,3), foreground:true),
                            new(Colors.Yellow, geometry: new(8,2,1,3), foreground:true),
                            new(Colors.Black, true),
                            new(Colors.White),
                        ],
                        border: Borders.Rounded,
                        x: -8
                    ),
                    new(
                        text:
                        """

                           ╭──╮   
                           │A_│   
                           ╰──╯   
                          Editor  

                        """,
                        defaultCharacter: ' ',
                        border: Borders.Rounded,
                        x:4

                    ),
            ]
        );
        int selection = 0;
        const int min = 0;
        const int max = 1;
        bool open = true;
        while (open)
        {
            while (Console.KeyAvailable)
            {
                ConsoleKeyInfo key = renderer.ReadKey();
                switch (key.Key)
                {
                    case ConsoleKey.RightArrow:
                        renderer.Object.SubObjects[selection].ColorAreas.RemoveRange(renderer.Object.SubObjects[selection].ColorAreas.Count - 2, 2);
                        selection = Math.Min(selection + 1, max);
                        renderer.Object.SubObjects[selection].ColorAreas.Add(new(Colors.Black, true));
                        renderer.Object.SubObjects[selection].ColorAreas.Add(new(Colors.White));
                        break;
                    case ConsoleKey.LeftArrow:
                        renderer.Object.SubObjects[selection].ColorAreas.RemoveRange(renderer.Object.SubObjects[selection].ColorAreas.Count - 2, 2);
                        selection = Math.Max(selection - 1, min);
                        renderer.Object.SubObjects[selection].ColorAreas.Add(new(Colors.Black, true));
                        renderer.Object.SubObjects[selection].ColorAreas.Add(new(Colors.White));
                        break;
                    case ConsoleKey.Enter:
                        open = false;
                        switch (selection)
                        {
                            case 0:
                                mode = "view";
                                break;
                            case 1:
                                mode = "edit";
                                break;
                        }
                        break;
                }
            }

            Thread.Sleep(50);
            if (renderer.UpdateScreenSize())
            {
                renderer.Object.Width = renderer.TerminalWidth;
                renderer.Object.Height = renderer.TerminalHeight;
            }
            renderer.Update();

        }
    }
}