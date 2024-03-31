using System;
using System.IO;
using System.Threading;
using AnsiRenderer;
namespace HeatManagement.CLI;

static partial class App
{
    static void RunEditorGreeter(Arguments arguments)
    {
        renderer.Object = new(
            geometry: new(0, 0, renderer.TerminalWidth, renderer.TerminalHeight),
            internalAlignmentX: Alignment.Center,
            internalAlignmentY: Alignment.Center,
            defaultCharacter: ' '
        );

        AssetManager? assets = null;
        SourceDataManager? sourceData = null;

        string editedFilepath = "";
        string? editedFileType = null;
        string tryLoadEditedFile(string filePath)
        {
            if (!File.Exists(filePath))
                try
                {
                    File.Create(filePath);
                    editedFilepath = filePath;
                    return "";
                }
                catch
                {
                    return "Cannot access or create a file at the edit path";
                }

            string text = "";
            try
            {
                text = File.ReadAllText(filePath);
            }
            catch
            {
                return "Edited file is not readable";
            }

            try
            {
                File.OpenWrite(filePath).Close();
            }
            catch
            {
                return "Edited file is unwritable";
            }

            if (text == "")
            {
                editedFilepath = filePath;
                return "";
            }
            else
            {
                try
                {
                    assets = new(text);
                    editedFileType = "asset";
                }
                catch
                {
                    try
                    {
                        sourceData = new(text);
                        editedFileType = "sourceData";
                    }
                    catch
                    {
                        return "Edited file already exists and contains unrelated data";
                    }
                }
            }
            editedFilepath = filePath;
            return "";
        }

        TextBox(
            text: arguments.EditPath ?? "data.json",
            title: "Input the assets or source data file path:",
            fileAction: tryLoadEditedFile,
            tryInitialAction: arguments.EditPath != null
        );

        if (editedFileType == null) EditTypeSelector(ref editedFileType);

        RunEditor(editedFileType!, editedFilepath, assets, sourceData);
    }

    private static void EditTypeSelector(ref string? editedFileType)
    {
        renderer.Object = new(
            defaultCharacter: ' ',
            geometry: new(0, 0, renderer.TerminalWidth, renderer.TerminalHeight),
            internalAlignmentX: Alignment.Center,
            internalAlignmentY: Alignment.Center,
            subObjects: [
                new(
                    animation:[
                    """

                        ▐▌
                       ▀▜▛▀
                        ▐▌
                        ▐▌
                      Assets  

                    """,
                    """

                       ▄ ▟▘
                        █▛▄
                       ▝▜▌
                        ▐▌
                      Assets  

                    """,
                    """

                       ▜▖▗▛
                       ▗██▖
                       ▀▐▌▀
                        ▐▌
                      Assets  

                    """,
                    """

                       ▝▙ ▄
                       ▄▜█
                        ▐▛▘
                        ▐▌
                      Assets  

                    """,
                    ],
                    defaultCharacter: ' ',
                    colorAreas:[
                        new(Colors.Black, true),
                        new(Colors.White),
                    ],
                    border: Borders.Rounded,
                    x: -8
                ),
                new(
                    text:
                    """

                        ▗▛
                        ▀▙▖
                        ▗▛
                      Source  
                       Data   

                    """,
                    colorAreas:[
                        new(Colors.Yellow, true, new(2,2,6,3)),
                    ],
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
                                editedFileType = "asset";
                                break;
                            case 1:
                                editedFileType = "sourceData";
                                break;
                        }
                        break;
                }
            }

            Thread.Sleep(50);
            renderer.Object.SubObjects[0].AnimationFrame++;


            if (renderer.UpdateScreenSize())
            {
                renderer.Object.Width = renderer.TerminalWidth;
                renderer.Object.Height = renderer.TerminalHeight;
            }
            renderer.Update();

        }
    }
}