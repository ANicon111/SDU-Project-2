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

        renderer.Object.SubObjects.Add(new(
            geometry: new(0, 0, renderer.TerminalWidth, renderer.TerminalHeight),
            defaultCharacter: ' ',
            colorAreas: [
                new(
                    color: Colors.Black.WithAlpha(0.75)
                )
            ],
            subObjects: [
                //header
                new(
                    geometry: new(0, 0, renderer.TerminalWidth, graphs.Count),
                    colorAreas:[
                        new(Colors.Black.WithAlpha(0.25))
                    ],
                    defaultCharacter: ' ',
                    subObjects:[
                        //name list
                        new(
                            geometry:new(0, 0, renderer.TerminalWidth, graphs.Count),
                            subObjects:[],
                            externalAlignmentX: Alignment.Left,
                            internalAlignmentX: Alignment.Left
                        ),
                        //value list
                        new(
                            geometry:new(0, 0, renderer.TerminalWidth, graphs.Count),
                            subObjects:[],
                            externalAlignmentX: Alignment.Center,
                            internalAlignmentX: Alignment.Center
                        ),
                        //time
                        new(
                            text: $"{times[selectedTime].Item1:dd'.'MM'.'yyyy' 'HH':'mm':'ss} - {times[selectedTime].Item2:dd'.'MM'.'yyyy' 'HH':'mm':'ss}",
                            colorAreas: [new(Colors.White,true)],
                            externalAlignmentX: Alignment.Right
                        ),
                    ]
                ),
                //selection bar
                new(
                    geometry: new(0, graphs.Count, graphs.Count, renderer.TerminalHeight - graphs.Count),
                    defaultCharacter: ' ',
                    colorAreas: [new(Colors.White.WithAlpha(0.5))]
                ),
                //zero line
                new(
                    geometry: new(0, zeroHeight + graphs.Count - 1, renderer.TerminalWidth, 1),
                    defaultCharacter: '▁'
                ),
                //graph bar list
                new(
                    geometry: new(0, graphs.Count, times.Count*graphs.Count, renderer.TerminalHeight - graphs.Count),
                    defaultCharacter:' '
                ),
                //helpers
                new(
                    text:
                    """
                              
                      Q  quit 
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
        ));

        //getters for readability
        RendererObject root() => renderer.Object.SubObjects[1];
        RendererObject header() => renderer.Object.SubObjects[1].SubObjects[0];
        RendererObject nameList() => renderer.Object.SubObjects[1].SubObjects[0].SubObjects[0];
        RendererObject valueList() => renderer.Object.SubObjects[1].SubObjects[0].SubObjects[1];
        RendererObject selectionBar() => renderer.Object.SubObjects[1].SubObjects[1];
        RendererObject zeroLine() => renderer.Object.SubObjects[1].SubObjects[2];
        RendererObject graphBarList() => renderer.Object.SubObjects[1].SubObjects[3];

        for (int i = 0; i < graphs.Count; i++)
        {
            nameList().SubObjects.Add(
                new(
                    y: i,
                    text: names[i],
                    colorAreas: [new(colors[i], true)]
                )
            );
            valueList().SubObjects.Add(
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
            graphBarList().ColorAreas.Add(new(grays[i % 2], false, new(i * graphs.Count, 0, graphs.Count, 100000)));
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
                graphBarList().SubObjects.Add(new(
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
                        new(Color.FromUInt(0), true, new(0, 0, 1, 1), Alignment.Center, Alignment.Bottom)
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
            graphBarList().X =
                -Math.Clamp(selectedTime * graphs.Count - renderer.TerminalWidth / 2, 0, Math.Max(times.Count * graphs.Count - renderer.TerminalWidth, 0));
            selectionBar().X = selectedTime * graphs.Count + graphBarList().X;

            //change the values
            for (int i = 0; i < graphs.Count; i++)
            {
                valueList().SubObjects[i] =
                new(
                    y: i,
                    text: $"{(graphs[i].TryGetValue(times[selectedTime], out double value) ? value : 0.0):0.##} {unitOfMeasurement}",
                    colorAreas: [new(colors[i], true)],
                    externalAlignmentX: Alignment.Center
                );
            }

            //change the time
            header().SubObjects[2] =
                new(
                    text: $"{times[selectedTime].Item1:dd'.'MM'.'yyyy' 'HH':'mm':'ss} - {times[selectedTime].Item2:dd'.'MM'.'yyyy' 'HH':'mm':'ss}",
                    colorAreas: [new(Colors.White, true)],
                    externalAlignmentX: Alignment.Right
                );


            //recalculate all geometries if terminal resized
            if (resize)
            {
                zeroHeight = Math.Max(graphs.Count, (int)((renderer.TerminalHeight - graphs.Count) * maxValue / (maxValue - minValue)));
                zeroLine().Width = renderer.TerminalWidth;
                zeroLine().Y = zeroHeight + graphs.Count - 1;
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
                        height *= sign;
                        int heightFractions8 = (int)((Math.Abs(renderer.TerminalHeight * graphValue / (maxValue - minValue)) - height) * 8);
                        graphBarList().SubObjects[i * graphs.Count + j] = new(
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
                                new(Color.FromUInt(0), true, new(0, 0, 1, 1), Alignment.Center, Alignment.Bottom)
                            ] : [new(colors[j], true)],
                            defaultCharacter: '█',
                            //modify the top and bottom borders for extra precision
                            border: new(
                                top: sign < 0 ? null : (char)('▁' + heightFractions8),
                                bottom: sign > 0 ? null : (char)('█' - heightFractions8)
                            )
                        );
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
                switch (renderer.ReadKey().Key)
                {
                    //switch highlighted graph bar and move graph to center it
                    case ConsoleKey.LeftArrow:
                        selectedTime = Math.Max(selectedTime - 1, 0);
                        updateGraph();
                        break;
                    case ConsoleKey.RightArrow:
                        selectedTime = Math.Min(selectedTime + 1, times.Count - 1);
                        updateGraph();
                        break;

                    //quit
                    case ConsoleKey.Q:
                        viewing = false;
                        break;

                    //export to csv/json
                    case ConsoleKey.E:
                        TextBox(
                            text: "graph.csv",
                            title: "Input the exported file path:",
                            fileAction: tryExportFile
                        );
                        break;
                }
            }

            Thread.Sleep(50);
            if (renderer.UpdateScreenSize())
            {
                zeroHeight = Math.Max(1, (int)((renderer.TerminalHeight - 1) * maxValue / (maxValue - minValue)));
                renderer.Object.Width = renderer.TerminalWidth;
                renderer.Object.Height = renderer.TerminalHeight;
                root().Width = renderer.TerminalWidth;
                root().Height = renderer.TerminalHeight;
                graphBarList().Height = renderer.TerminalHeight - graphs.Count;
                selectionBar().Height = renderer.TerminalHeight - graphs.Count;
                header().Width = renderer.TerminalWidth;
                nameList().Width = renderer.TerminalWidth;
                valueList().Width = renderer.TerminalWidth;
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