using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading;
using AnsiRenderer;

namespace HeatManagement;

static partial class CLI
{
    private struct JsonData(DateTime startTime, DateTime endTime, double value)
    {
        public DateTime StartTime { get; set; } = startTime;
        public DateTime EndTime { get; set; } = endTime;
        public double Value { get; set; } = value;
    }

    static void GraphDrawer(string name, string unitOfMeasurement, Color color, List<Tuple<DateTime, DateTime>> times, Dictionary<Tuple<DateTime, DateTime>, double> values)
    {
        ColorArea[] accented = [new(color, true)];
        int selectedTime = 0;
        //small maxValue set to avoid 0/0 in formulas
        double maxValue = 1e-10;
        double minValue = 0;

        foreach (KeyValuePair<Tuple<DateTime, DateTime>, double> val in values)
        {
            if (maxValue < val.Value) maxValue = val.Value;
            if (minValue > val.Value) minValue = val.Value;
        }

        int zeroHeight = Math.Max(1, (int)((renderer.TerminalHeight - 1) * maxValue / (maxValue - minValue)));

        renderer.Object.SubObjects.Add(new(
            geometry: new(0, 0, renderer.TerminalWidth, renderer.TerminalHeight),
            defaultCharacter: ' ',
            colorAreas: [
                new(
                    color: Colors.Black.WithAlpha(0.75)
                )
            ],
            subObjects: [
                new(
                    text: name,
                    colorAreas: accented,
                    externalAlignmentX:Alignment.Left
                ),
                new(
                    text: $"{(values.TryGetValue(times[selectedTime], out double value) ? value : 0.0)} {unitOfMeasurement}",
                    colorAreas: accented,
                    externalAlignmentX: Alignment.Center
                ),
                new(
                    text: $"{times[selectedTime].Item1} - {times[selectedTime].Item2}",
                    colorAreas: accented,
                    externalAlignmentX: Alignment.Right
                ),
                new(
                    geometry: new(0, Math.Max(1,Math.Max(1,(int)((renderer.TerminalHeight - 1) * maxValue / (maxValue - minValue)))), renderer.TerminalWidth, 1),
                    defaultCharacter: '▁'
                ),
                new(
                    geometry: new(0, 1, times.Count, renderer.TerminalHeight - 1)
                ),
                new(
                    geometry: new(0, 1, 1, renderer.TerminalHeight - 1),
                    defaultCharacter: ' ',
                    colorAreas: [new(color.WithAlpha(0.75))]
                ),
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
                                        
                      E export to json  
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

        //simple getter functions for readability
        RendererObject graphRendererObject() => renderer.Object.SubObjects[1];

        //add graph bars
        for (int i = 0; i < times.Count; i++)
        {
            double graphValue = 0;
            if (values.TryGetValue(times[i], out double val))
            {
                graphValue = val;
            }
            int height = (int)(renderer.TerminalHeight * graphValue / (maxValue - minValue));
            graphRendererObject().SubObjects[4].SubObjects.Add(new(
                geometry: new(
                    i,
                    Math.Min(zeroHeight - height, zeroHeight),
                    1,
                    Math.Abs(height)
                ),
                defaultCharacter: '█'
            ));
        }

        //initial render
        renderer.Update();

        void updateGraph(bool resize = false)
        {
            //set graph position
            graphRendererObject().SubObjects[4].X =
                -Math.Clamp(selectedTime - renderer.TerminalWidth / 2, 0, Math.Max(times.Count - renderer.TerminalWidth, 0));
            graphRendererObject().SubObjects[5].X = selectedTime + graphRendererObject().SubObjects[4].X;

            //change the value and time
            graphRendererObject().SubObjects[1] =
            new(
                text: $"{(values.TryGetValue(times[selectedTime], out double value) ? value : 0.0)} {unitOfMeasurement}",
                colorAreas: accented,
                externalAlignmentX: Alignment.Center
            );

            graphRendererObject().SubObjects[2] =
            new(
                text: $"{times[selectedTime].Item1} - {times[selectedTime].Item2}",
                colorAreas: accented,
                externalAlignmentX: Alignment.Right
            );


            //recalculate all geometries if terminal resized
            if (resize)
            {
                graphRendererObject().SubObjects[3].Width = renderer.TerminalWidth;
                graphRendererObject().SubObjects[3].Y = zeroHeight;

                for (int i = 0; i < times.Count; i++)
                {
                    double graphValue = 0;
                    if (values.TryGetValue(times[i], out double val))
                    {
                        graphValue = val;
                    }

                    int height = (int)(renderer.TerminalHeight * graphValue / (maxValue - minValue));
                    graphRendererObject().SubObjects[4].SubObjects[i].Y = Math.Min(zeroHeight - height, zeroHeight);
                    graphRendererObject().SubObjects[4].SubObjects[i].Height = Math.Abs(height);
                }
            }
        }

        //create the json exportable dictionary
        List<JsonData> jsonValues = [];
        foreach (var val in values)
        {
            jsonValues.Add(new(val.Key.Item1, val.Key.Item2, val.Value));
        }

        //json export function for the greeter file dialogue
        JsonSerializerOptions jsonOptions = new() { WriteIndented = true };
        string tryExportFile(string filePath)
        {
            try
            {
                File.WriteAllText(filePath, JsonSerializer.Serialize(jsonValues, jsonOptions));
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

                    //export to json
                    case ConsoleKey.E:
                        FilePathMenu(
                            args: ref _notArgs,
                            filePath: $"{name.ToLower()}.json",
                            title: "Input the exported file path:",
                            tryLoadFile: tryExportFile
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
                graphRendererObject().Width = renderer.TerminalWidth;
                graphRendererObject().Height = renderer.TerminalHeight;
                graphRendererObject().SubObjects[4].Height = renderer.TerminalHeight;
                graphRendererObject().SubObjects[5].Height = renderer.TerminalHeight - 1;
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