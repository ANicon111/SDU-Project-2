using System;
using System.Collections.Generic;
using System.Threading;
using AnsiRenderer;

namespace HeatManagement;

static partial class CLI
{
    static void RunGraphList(AssetManager assetManager, SourceDataManager sourceDataManager)
    {
        ResultDataManager resultDataManager = new();
        Optimizer.GetResult(assetManager, sourceDataManager, resultDataManager);


        List<string> resourceOptions = [
            "Cost",
            "Electricity",
            "CO2",
        ];

        List<string> assetOptions = [];


        List<Tuple<DateTime, DateTime>> times = [.. resultDataManager.ResultData.Keys];

        List<Color> colors =
        [
            Colors.Lime,
            Colors.Yellow,
            Colors.Gray,
            Colors.Crimson,
            Colors.LawnGreen,
            Colors.SkyBlue,
            Colors.BlanchedAlmond,
            Colors.Sienna,
            Colors.Purple,
            Colors.Goldenrod,
            Colors.BlueViolet,
            Colors.PeachPuff,
        ];

        List<string> resourceMeasurements =
        [
            "dkk",
            "MWh",
            "kg",
        ];
        List<string> assetMeasurements = [];

        Dictionary<string, Dictionary<Tuple<DateTime, DateTime>, double>> graphValues = new(){
            {"Cost", []},
            {"Electricity", []},
            {"CO2", []},
        };

        //get individual asset usage stats
        foreach (var result in resultDataManager.ResultData)
        {
            graphValues["Cost"][result.Key] = 0;
            graphValues["Electricity"][result.Key] = 0;
            graphValues["CO2"][result.Key] = 0;

            foreach (KeyValuePair<string, ResultData> assetResult in result.Value)
            {
                //format asset name
                string assetName = assetResult.Key.ToUpper();

                //add hardcoded values
                graphValues["Cost"][result.Key] += assetResult.Value.Cost;
                graphValues["Electricity"][result.Key] += assetResult.Value.Electricity;
                graphValues["CO2"][result.Key] += assetResult.Value.CO2;

                //initialize hardcoded asset values
                if (!graphValues.ContainsKey($"{assetName}-Cost"))
                {
                    assetOptions.Add($"{assetName}-Cost");
                    graphValues.Add($"{assetName}-Cost", []);
                    assetMeasurements.Add("dkk");
                    assetOptions.Add($"{assetName}-Electricity");
                    graphValues.Add($"{assetName}-Electricity", []);
                    assetMeasurements.Add("MWh");
                    assetOptions.Add($"{assetName}-CO2");
                    graphValues.Add($"{assetName}-CO2", []);
                    assetMeasurements.Add("kg");
                }

                //set the hardcoded asset values
                graphValues[$"{assetName}-Cost"][result.Key] = assetResult.Value.Cost;
                graphValues[$"{assetName}-Electricity"][result.Key] = assetResult.Value.Electricity;
                graphValues[$"{assetName}-CO2"][result.Key] = assetResult.Value.CO2;

                //get additional resource usage
                foreach (KeyValuePair<string, double> additionalResource in assetResult.Value.AdditionalResources)
                {
                    //format resource name
                    string resourceName = additionalResource.Key[..1].ToUpper() + additionalResource.Key[1..].ToLower();

                    //initialize and add resource value
                    if (!resourceOptions.Contains(resourceName))
                    {
                        resourceOptions.Add(resourceName);
                        graphValues.Add(resourceName, []);
                        resourceMeasurements.Add("MWh");
                    }
                    if (!graphValues[resourceName].ContainsKey(result.Key)) graphValues[resourceName][result.Key] = 0;
                    graphValues[resourceName][result.Key] += additionalResource.Value;

                    //initialize and set asset resource usage
                    if (!assetOptions.Contains($"{assetName}-{resourceName}"))
                    {
                        assetOptions.Add($"{assetName}-{resourceName}");
                        graphValues.Add($"{assetName}-{resourceName}", []);
                        assetMeasurements.Add("MWh");
                    }
                    graphValues[$"{assetName}-{resourceName}"][result.Key] = additionalResource.Value;
                }
            }

        }

        //merge options and units of measurements
        string[] options = [.. resourceOptions, .. assetOptions];
        string[] measurements = [.. resourceMeasurements, .. assetMeasurements];

        //create a RendererObject for each 
        RendererObject[] graphs = new RendererObject[options.Length];

        for (int i = 0; i < options.Length; i++)
        {
            graphs[i] = new(
                text: options[i],
                y: i + 5

            );
        }

        int selectedGraph = 0;
        renderer.Object = new(
            geometry: new(0, 0, renderer.TerminalWidth, renderer.TerminalHeight),
            subObjects: [
                new(
                    subObjects: [
                        new(
                            text:
                            """
                             ↑ ↓   Change selected graph
                            ENTER  Display selected graph
                              Q    Quit the application
                            """,
                            externalAlignmentX:Alignment.Center,
                            y:1
                        ),
                        ..graphs,
                    ],
                    internalAlignmentX: Alignment.Center,
                    externalAlignmentX: Alignment.Center,
                    border: Borders.Rounded
                )
            ]
        );
        //removing empty space at the end of the list
        renderer.Object.SubObjects[0].Height--;

        graphs[selectedGraph].ColorAreas = [new(color: colors[selectedGraph % colors.Count]), new(color: Colors.Black, foreground: true)];
        renderer.Update(forceRedraw: true);
        bool running = true;
        while (running)
        {
            while (Console.KeyAvailable)
            {
                switch (renderer.ReadKey().Key)
                {
                    //switch selected graph and move the menu to center the selection
                    case ConsoleKey.UpArrow:
                        graphs[selectedGraph].ColorAreas = [];
                        selectedGraph = Math.Max(selectedGraph - 1, 0);
                        renderer.Object.SubObjects[0].Y =
                            -Math.Clamp(selectedGraph - renderer.TerminalHeight / 2 + 4, 0, Math.Max(options.Length + 6 - renderer.TerminalHeight, 0));
                        graphs[selectedGraph].ColorAreas =
                            [new(color: colors[selectedGraph % colors.Count]), new(color: Colors.Black, foreground: true)];
                        break;
                    case ConsoleKey.DownArrow:
                        graphs[selectedGraph].ColorAreas = [];
                        selectedGraph = Math.Min(selectedGraph + 1, graphs.Length - 1);
                        renderer.Object.SubObjects[0].Y =
                            -Math.Clamp(selectedGraph - renderer.TerminalHeight / 2 + 4, 0, Math.Max(options.Length + 6 - renderer.TerminalHeight, 0));
                        graphs[selectedGraph].ColorAreas =
                            [new(color: colors[selectedGraph % colors.Count]), new(color: Colors.Black, foreground: true)];
                        break;
                    case ConsoleKey.Enter:
                        GraphDrawer(
                            options[selectedGraph],
                            measurements[selectedGraph],
                            colors[selectedGraph % colors.Count],
                            times,
                            graphValues[options[selectedGraph]]
                        );
                        break;
                    case ConsoleKey.Q:
                        running = false;
                        break;

                }
            }

            Thread.Sleep(50);
            if (renderer.UpdateScreenSize())
            {
                renderer.Object.Width = renderer.TerminalWidth;
                renderer.Object.Height = renderer.TerminalHeight;
                renderer.Object.SubObjects[0].Y = -Math.Clamp(selectedGraph - renderer.TerminalHeight / 2 + 4, 0, Math.Max(options.Length + 5 - renderer.TerminalHeight, 0));
            }
            renderer.Update();
        }
        renderer.Reset();
        Console.Clear();
        Console.SetCursorPosition(0, 0);
    }
}