using System;
using System.Collections.Generic;
using System.Threading;
using AnsiRenderer;

namespace HeatManagement;

static partial class CLI
{
    static void GraphDrawer(string name, string unitOfMeasurement, Color color, List<Tuple<DateTime, DateTime>> times, Dictionary<Tuple<DateTime, DateTime>, double> values)
    {
        ColorArea[] accented = [new(color: color, foreground: true)];
        int selectedTime = 0;
        double maxValue = 0.001;
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
                    colorAreas: [.. accented],
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
                    text:
                    """
                              
                      Q  quit 
                    """,
                    colorAreas:[
                        new(color:Colors.DarkRed.WithAlpha(0.75)),
                        new(color:Colors.White, foreground:true),
                    ],
                    externalAlignmentX:Alignment.Right,
                    externalAlignmentY:Alignment.Bottom
                ),
                new(
                    text:
                    """
                                                
                     ← →  Change selected time  
                    """,
                    colorAreas:[
                        new(color:Colors.DarkBlue.WithAlpha(0.75)),
                        new(color:Colors.White, foreground:true),
                    ],
                    externalAlignmentX:Alignment.Left,
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
        //accent the (initially) selected graph bar
        graphRendererObject().SubObjects[4].SubObjects[selectedTime].ColorAreas = [.. accented];

        //initial render
        renderer.Update();

        void updateGraph(bool resize = false)
        {
            //set graph position
            graphRendererObject().SubObjects[4].X =
                -Math.Clamp(selectedTime - renderer.TerminalWidth / 2, 0, Math.Max(times.Count - renderer.TerminalWidth, 0));

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
                        graphRendererObject().SubObjects[4].SubObjects[selectedTime].ColorAreas = [];
                        selectedTime = Math.Max(selectedTime - 1, 0);
                        graphRendererObject().SubObjects[4].SubObjects[selectedTime].ColorAreas = [.. accented];
                        updateGraph();
                        break;
                    case ConsoleKey.RightArrow:
                        graphRendererObject().SubObjects[4].SubObjects[selectedTime].ColorAreas = [];
                        selectedTime = Math.Min(selectedTime + 1, times.Count - 1);
                        graphRendererObject().SubObjects[4].SubObjects[selectedTime].ColorAreas = [.. accented];
                        updateGraph();
                        break;

                    case ConsoleKey.Q:
                        viewing = false;
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