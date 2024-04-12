using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using AnsiRenderer;

namespace HeatManagement.CLI;

static partial class App
{
    private struct JsonData(DateTime startTime, DateTime endTime, SortedDictionary<string, double> values)
    {
        public DateTime StartTime { get; set; } = startTime;
        public DateTime EndTime { get; set; } = endTime;
        public SortedDictionary<string, double> Values { get; set; } = values;
    }

    static void GraphDrawer(List<string> names, string unitOfMeasurement, List<Color> colors, List<Tuple<DateTime, DateTime>> times, List<Dictionary<Tuple<DateTime, DateTime>, double>> graphs)
    {
        int selectedTime = 0;
        //small maxValue set to avoid 0/0 in formulas
        double maxValue = 1e-10;
        double minValue = 0;

        foreach (Dictionary<Tuple<DateTime, DateTime>, double> graph in graphs)
        {
            foreach (KeyValuePair<Tuple<DateTime, DateTime>, double> val in graph)
            {
                if (maxValue < val.Value) maxValue = val.Value;
                if (minValue > val.Value) minValue = val.Value;
            }
        }

        int zeroHeight = Math.Max(1, (int)((renderer.TerminalHeight - graphs.Count) * maxValue / (maxValue - minValue)));

        RendererObject nameList = new(
            geometry: new(0, 0, renderer.TerminalWidth, graphs.Count),
            subObjects: [],
            externalAlignmentX: Alignment.Left,
            internalAlignmentX: Alignment.Left
        );

        RendererObject valueList = new(
            geometry: new(0, 0, renderer.TerminalWidth, graphs.Count),
            subObjects: [],
            externalAlignmentX: Alignment.Center,
            internalAlignmentX: Alignment.Center
        );

        RendererObject timeSpan = new(
            text: $"{times[selectedTime].Item1:yyyy'-'MM'-'dd' 'HH':'mm':'ss} - {times[selectedTime].Item2:yyyy'-'MM'-'dd' 'HH':'mm':'ss}",
            colorAreas: [new(Colors.White, true)],
            externalAlignmentX: Alignment.Right
        );

        RendererObject header = new(
            geometry: new(0, 0, renderer.TerminalWidth, graphs.Count),
            colorAreas: [
                new(Colors.Black.WithAlpha(0.25))
            ],
            defaultCharacter: ' ',
            subObjects: [
                nameList,
                valueList,
                timeSpan
            ]
        );

        RendererObject selectionBar = new(
            geometry: new(0, graphs.Count, graphs.Count, renderer.TerminalHeight - graphs.Count),
            defaultCharacter: ' ',
            colorAreas: [new(Colors.White.WithAlpha(0.5))]
        );

        RendererObject zeroLine = new(
            geometry: new(0, zeroHeight + graphs.Count - 1, renderer.TerminalWidth, 1),
            defaultCharacter: '▁'
        );

        RendererObject graphBarList = new(
            geometry: new(0, graphs.Count, times.Count * graphs.Count, renderer.TerminalHeight - graphs.Count),
            defaultCharacter: ' '
        );

        RendererObject root = new(
            geometry: new(0, 0, renderer.TerminalWidth, renderer.TerminalHeight),
            defaultCharacter: ' ',
            colorAreas: [
                new(
                    color: Colors.Black.WithAlpha(0.75)
                )
            ],
            subObjects: [
                header,
                selectionBar,
                zeroLine,
                graphBarList,
                //helpers
                new(
                    text:
                    """
                                
                      ESC  quit 
                    """,
                    colorAreas:[
                        new(color:Colors.Maroon.WithAlpha(0.75)),
                        new(color:Colors.White, foreground:true),
                    ],
                    externalAlignmentX:Alignment.Right,
                    externalAlignmentY:Alignment.Bottom
                ),
                new(
                    text:
                    """
                                                
                     ← →  change selected time  
                    """,
                    colorAreas:[
                        new(color:Colors.Navy.WithAlpha(0.75)),
                        new(color:Colors.White, foreground:true),
                    ],
                    externalAlignmentX:Alignment.Left,
                    externalAlignmentY:Alignment.Bottom
                ),
                new(
                    text:
                    """
                                
                      E export  
                    """,
                    colorAreas:[
                        new(color:Colors.Green.WithAlpha(0.75)),
                        new(color:Colors.White, foreground:true),
                    ],
                    externalAlignmentX:Alignment.Center,
                    externalAlignmentY:Alignment.Bottom
                ),
            ]
        );

        renderer.Object.SubObjects.Add(root);

        for (int i = 0; i < graphs.Count; i++)
        {
            nameList.SubObjects.Add(
                new(
                    y: i,
                    text: names[i],
                    colorAreas: [new(colors[i], true)]
                )
            );
            valueList.SubObjects.Add(
                new(
                    y: i,
                    text: $"{(graphs[i].TryGetValue(times[selectedTime], out double value) ? value : 0.0):0.##} {unitOfMeasurement}",
                    colorAreas: [new(colors[i], true)]
                )
            );
        }

        //alternating background grays for readability
        Color[] grays = [
            Colors.DarkGray.WithAlpha(0.25),
            Colors.LightGray.WithAlpha(0.25)
        ];

        //add graph bars
        for (int i = 0; i < times.Count; i++)
        {
            graphBarList.ColorAreas.Add(new(
                grays[i % 2],
                false,
                new(i * graphs.Count, 0, graphs.Count, 100000)
                ));
            for (int j = 0; j < graphs.Count; j++)
            {
                double graphValue = 0;
                if (graphs[j].TryGetValue(times[i], out double val))
                {
                    graphValue = val;
                }
                int height = (int)(renderer.TerminalHeight * graphValue / (maxValue - minValue));
                int sign = Math.Sign(height);
                height *= sign;
                int heightFractions8 = (int)((Math.Abs(renderer.TerminalHeight * graphValue / (maxValue - minValue)) - height) * 8);
                graphBarList.SubObjects.Add(new(
                    geometry: new(
                        i * graphs.Count + j,
                        sign > 0 ? zeroHeight - height : zeroHeight,
                        1,
                        height
                    ),
                    //invert bottom slice if graph value is negative
                    colorAreas: sign < 0 ?
                    [
                        new(colors[j], true),
                        new(colors[j], false, new(0, 0, 1, 1), Alignment.Center, Alignment.Bottom),
                        new(Color.FromUInt(0), true, new(0, 0, 1, 1), Alignment.Center, Alignment.Bottom, true)
                    ] : [new(colors[j], true)],
                    defaultCharacter: '█',
                    //modify the top and bottom borders for extra precision
                    border: new(
                        top: sign < 0 ? null : (char)('▁' + heightFractions8),
                        bottom: sign > 0 ? null : (char)('█' - heightFractions8)
                    )
                ));
            }
        }

        //initial render
        renderer.Update();

        void updateGraph(bool resize = false)
        {
            //set graph position
            graphBarList.X =
                -Math.Clamp(selectedTime * graphs.Count - renderer.TerminalWidth / 2, 0, Math.Max(times.Count * graphs.Count - renderer.TerminalWidth, 0));
            selectionBar.X = selectedTime * graphs.Count + graphBarList.X;

            //change the values
            for (int i = 0; i < graphs.Count; i++)
            {
                valueList.SubObjects[i].Text = $"{(graphs[i].TryGetValue(times[selectedTime], out double value) ? value : 0.0):0.##} {unitOfMeasurement}";
            }

            //change the time
            timeSpan.Text = $"{times[selectedTime].Item1:yyyy'-'MM'-'dd' 'HH':'mm':'ss} - {times[selectedTime].Item2:yyyy'-'MM'-'dd' 'HH':'mm':'ss}";


            //recalculate all geometries if terminal resized
            if (resize)
            {
                zeroHeight = Math.Max(1, (int)((renderer.TerminalHeight - graphs.Count) * maxValue / (maxValue - minValue)));
                zeroLine.Width = renderer.TerminalWidth;
                zeroLine.Y = zeroHeight + graphs.Count - 1;
                for (int i = 0; i < times.Count; i++)
                {
                    for (int j = 0; j < graphs.Count; j++)
                    {
                        double graphValue = 0;
                        if (graphs[j].TryGetValue(times[i], out double val))
                        {
                            graphValue = val;
                        }
                        int height = (int)(renderer.TerminalHeight * graphValue / (maxValue - minValue));
                        int sign = Math.Sign(height);
                        graphBarList.SubObjects[i * graphs.Count + j].Height = Math.Abs(height);
                        graphBarList.SubObjects[i * graphs.Count + j].Y = sign > 0 ? zeroHeight - height : zeroHeight;
                        int heightFractions8 = (int)((Math.Abs(renderer.TerminalHeight * graphValue / (maxValue - minValue)) - Math.Abs(height)) * 8);
                        graphBarList.SubObjects[i * graphs.Count + j].Border = new(
                                top: sign < 0 ? null : (char)('▁' + heightFractions8),
                                bottom: sign > 0 ? null : (char)('█' - heightFractions8)
                            );
                        graphBarList.SubObjects[i * graphs.Count + j].ColorAreas = sign < 0 ?
                            [
                                new(colors[j], true),
                                new(colors[j], false, new(0, 0, 1, 1), Alignment.Center, Alignment.Bottom),
                                new(Color.FromUInt(0), true, new(0, 0, 1, 1), Alignment.Center, Alignment.Bottom)
                            ] : [new(colors[j], true)];
                    }
                }
            }
        }

        //create the json exportable dictionary and the csv table
        List<string> csvHeader = ["StartTime", "EndTime"];
        for (int j = 0; j < names.Count; j++) csvHeader.Add(names[j]);
        List<string[]> csvTable = [[.. csvHeader]];
        List<JsonData> jsonValues = [];
        for (int i = 0; i < times.Count; i++)
        {
            jsonValues.Add(new(times[i].Item1, times[i].Item2, []));
            string[] csvRow = new string[csvTable[0].Length];
            csvRow[0] = $"{times[i].Item1}";
            csvRow[1] = $"{times[i].Item2}";
            for (int j = 0; j < graphs.Count; j++)
            {
                graphs[j].TryGetValue(times[i], out double value);
                jsonValues[i].Values.Add(names[j], value);
                csvRow[2 + j] = $"{value}";
            }
            csvTable.Add(csvRow);
        }

        //json/csv export function for the greeter file dialogue
        JsonSerializerOptions jsonOptions = new() { WriteIndented = true };
        string tryExportFile(string filePath)
        {
            try
            {
                if (filePath.Split('.').Last().ToLower() == "json")
                    File.WriteAllText(filePath, JsonSerializer.Serialize(jsonValues, jsonOptions));
                else
                    File.WriteAllText(filePath, Utils.TableToCSV(csvTable));
            }
            catch
            {
                return "Could not write to the selected path";
            }
            return "";
        }
        //funky trick to reuse the greeter file dialogue without using an argument
        string[] _notArgs = [];


        bool viewing = true;
        while (viewing)
        {
            //process keys while they are available in the buffer
            while (Console.KeyAvailable)
            {
                ConsoleKeyInfo consoleKeyInfo = renderer.ReadKey();
                int moveSpace = consoleKeyInfo.Modifiers == ConsoleModifiers.Control ? 5 : 1;
                switch (consoleKeyInfo.Key)
                {
                    //switch highlighted graph bar and move graph to center it
                    case ConsoleKey.LeftArrow:
                        selectedTime = Math.Max(selectedTime - moveSpace, 0);
                        updateGraph();
                        break;
                    case ConsoleKey.RightArrow:
                        selectedTime = Math.Min(selectedTime + moveSpace, times.Count - 1);
                        updateGraph();
                        break;

                    //quit
                    case ConsoleKey.Escape:
                        viewing = false;
                        break;

                    //export to csv/json
                    case ConsoleKey.E:
                        bool escaped = false;
                        TextBox(
                            escaped: ref escaped,
                            text: "graph.csv",
                            title: "Input the exported file path:",
                            fileAction: tryExportFile
                        );
                        break;
                }
            }

            Thread.Sleep(25);
            if (renderer.UpdateScreenSize())
            {
                zeroHeight = Math.Max(1, (int)((renderer.TerminalHeight - 1) * maxValue / (maxValue - minValue)));
                renderer.Object.Width = renderer.TerminalWidth;
                renderer.Object.Height = renderer.TerminalHeight;
                root.Width = renderer.TerminalWidth;
                root.Height = renderer.TerminalHeight;
                graphBarList.Height = renderer.TerminalHeight - graphs.Count;
                selectionBar.Height = renderer.TerminalHeight - graphs.Count;
                header.Width = renderer.TerminalWidth;
                nameList.Width = renderer.TerminalWidth;
                valueList.Width = renderer.TerminalWidth;
                updateGraph(true);
            }
            renderer.Update();
        }
        renderer.Object.SubObjects.RemoveAt(1);

        //for some reason, a double redraw is necessary to avoid a color spill on the main menu
        renderer.Update(forceRedraw: true);
        renderer.Update(forceRedraw: true);
    }
}