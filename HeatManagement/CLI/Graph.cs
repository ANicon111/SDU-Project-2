using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using AnsiRenderer;

namespace HeatManagement;

static partial class CLI
{
    public static void GraphDrawer(string name, string unitOfMeasurement, Color color, List<Tuple<DateTime, DateTime>> times, Dictionary<Tuple<DateTime, DateTime>, double> values)
    {
        int selectedTime = 0;
        double maxValue = 0.001;
        double minValue = 0;
        foreach (KeyValuePair<Tuple<DateTime, DateTime>, double> val in values)
        {
            if (maxValue < val.Value) maxValue = val.Value;
            if (minValue > val.Value) minValue = val.Value;
        }
        int zeroHeight = Math.Max(1, (int)((renderer.TerminalHeight - 1) * maxValue / (maxValue - minValue)));
        ColorArea[] accented = [new(color: color, foreground: true)];
        renderer.Object.SubObjects.Add(new(
            geometry: new(0, 0, renderer.TerminalWidth, renderer.TerminalHeight),
            defaultCharacter: ' ',
            colorAreas: [
                new(
                    color: Colors.Black.WithAlpha(0.5)
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
                )
            ]
        ));
        for (int i = 0; i < times.Count; i++)
        {
            double graphValue = 0;
            if (values.TryGetValue(times[i], out double val))
            {
                graphValue = val;
            }
            int height = (int)(renderer.TerminalHeight * graphValue / (maxValue - minValue));
            renderer.Object.SubObjects[1].SubObjects[4].SubObjects.Add(
            new(
                geometry: new(
                    i,
                    height > 0 ? zeroHeight - height : zeroHeight,
                    1,
                    Math.Abs(height)
                ),
                defaultCharacter: '█'
                )
            );
        }
        renderer.Object.SubObjects[1].SubObjects[4].SubObjects[selectedTime].ColorAreas = [.. accented];

        renderer.Update();
        void updateGraph(bool resize = false)
        {
            renderer.Object.SubObjects[1].SubObjects[1] =
            new(
                text: $"{(values.TryGetValue(times[selectedTime], out double value) ? value : 0.0)} {unitOfMeasurement}",
                colorAreas: accented,
                externalAlignmentX: Alignment.Center
            );
            renderer.Object.SubObjects[1].SubObjects[2] =
            new(
                text: $"{times[selectedTime].Item1} - {times[selectedTime].Item2}",
                colorAreas: accented,
                externalAlignmentX: Alignment.Right
            );
            renderer.Object.SubObjects[1].SubObjects[3] =
            new(
                geometry: new(0, zeroHeight, renderer.TerminalWidth, 1),
                defaultCharacter: '▁'
            );
            if (resize)
                for (int i = 0; i < times.Count; i++)
                {
                    double graphValue = 0;
                    if (values.TryGetValue(times[i], out double val))
                    {
                        graphValue = val;
                    }
                    int height = (int)(renderer.TerminalHeight * graphValue / (maxValue - minValue));
                    renderer.Object.SubObjects[1].SubObjects[4].SubObjects[i].Y = height > 0 ? zeroHeight - height : zeroHeight;
                    renderer.Object.SubObjects[1].SubObjects[4].SubObjects[i].Height = Math.Abs(height);
                }
        }
        bool viewing = true;
        while (viewing)
        {
            if (Console.KeyAvailable)
            {
                switch (renderer.ReadKey().Key)
                {
                    case ConsoleKey.LeftArrow:
                        renderer.Object.SubObjects[1].SubObjects[4].SubObjects[selectedTime].ColorAreas = [];
                        selectedTime = Math.Max(selectedTime - 1, 0);
                        if (-Math.Clamp(selectedTime - renderer.TerminalWidth / 2, 0, Math.Max(times.Count - renderer.TerminalWidth, 0)) != renderer.Object.SubObjects[1].SubObjects[4].X)
                            renderer.Object.SubObjects[1].SubObjects[4].X = -Math.Clamp(selectedTime - renderer.TerminalWidth / 2, 0, Math.Max(times.Count - renderer.TerminalWidth, 0));
                        renderer.Object.SubObjects[1].SubObjects[4].SubObjects[selectedTime].ColorAreas = [.. accented];
                        updateGraph();
                        break;
                    case ConsoleKey.RightArrow:
                        renderer.Object.SubObjects[1].SubObjects[4].SubObjects[selectedTime].ColorAreas = [];
                        selectedTime = Math.Min(selectedTime + 1, times.Count - 1);
                        if (-Math.Clamp(selectedTime - renderer.TerminalWidth / 2, 0, Math.Max(times.Count - renderer.TerminalWidth, 0)) != renderer.Object.SubObjects[1].SubObjects[4].X)
                            renderer.Object.SubObjects[1].SubObjects[4].X = -Math.Clamp(selectedTime - renderer.TerminalWidth / 2, 0, Math.Max(times.Count - renderer.TerminalWidth, 0));
                        renderer.Object.SubObjects[1].SubObjects[4].SubObjects[selectedTime].ColorAreas = [.. accented];
                        updateGraph();
                        break;
                    case ConsoleKey.Q:
                        viewing = false;
                        break;
                }
                while (Console.KeyAvailable) renderer.ReadKey();
            }
            Thread.Sleep(50);

            if (renderer.UpdateScreenSize())
            {
                zeroHeight = Math.Max(1, (int)((renderer.TerminalHeight - 1) * maxValue / (maxValue - minValue)));
                renderer.Object.Width = renderer.TerminalWidth;
                renderer.Object.Height = renderer.TerminalHeight;
                renderer.Object.SubObjects[1].Width = renderer.TerminalWidth;
                renderer.Object.SubObjects[1].Height = renderer.TerminalHeight;
                renderer.Object.SubObjects[1].SubObjects[4].Height = renderer.TerminalHeight;
                updateGraph(true);
            }
            renderer.Update();
        }
        renderer.Object.SubObjects.RemoveAt(1);
        //sure ._.
        renderer.Update(forceRedraw: true);
        renderer.Update(forceRedraw: true);
    }
}