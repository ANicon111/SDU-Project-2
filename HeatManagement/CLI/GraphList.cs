using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading;
using AnsiRenderer;

namespace HeatManagement.CLI;

static partial class App
{
    static void RunGraphList(AssetManager? assetManager, SourceDataManager? sourceDataManager = null, ResultDataManager? resultDataManager = null)
    {
        if (assetManager != null && sourceDataManager != null)
        {
            resultDataManager = new();
            Optimizer.GetResult(assetManager, sourceDataManager, resultDataManager);
        }

        List<string> resourceOptions = [
            "Cost",
            "Electricity",
            "CO2",
        ];

        List<string> assetOptions = [];


        List<Tuple<DateTime, DateTime>> times = [.. resultDataManager!.Data.Keys];

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
        foreach (KeyValuePair<Tuple<DateTime, DateTime>, Dictionary<string, ResultData>> result in resultDataManager.Data)
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
                foreach (KeyValuePair<string, AdditionalResource> additionalResource in assetResult.Value.AdditionalResources)
                {
                    //format resource name
                    string resourceName = additionalResource.Key[..1].ToUpper() + additionalResource.Key[1..].ToLower();

                    //initialize and add resource value
                    if (!resourceOptions.Contains(resourceName))
                    {
                        resourceOptions.Add(resourceName);
                        graphValues.Add(resourceName, []);
                        resourceMeasurements.Add(additionalResource.Value.Measurement);
                    }
                    if (!graphValues[resourceName].ContainsKey(result.Key)) graphValues[resourceName][result.Key] = 0;
                    graphValues[resourceName][result.Key] += additionalResource.Value.Value;

                    //initialize and set asset resource usage
                    if (!assetOptions.Contains($"{assetName}-{resourceName}"))
                    {
                        assetOptions.Add($"{assetName}-{resourceName}");
                        graphValues.Add($"{assetName}-{resourceName}", []);
                        assetMeasurements.Add(additionalResource.Value.Measurement);
                    }
                    graphValues[$"{assetName}-{resourceName}"][result.Key] = additionalResource.Value.Value;
                }
            }
        }

        //merge options and units of measurements
        string[] options = [.. resourceOptions, .. assetOptions];
        string[] measurements = [.. resourceMeasurements, .. assetMeasurements];
        List<Color> colorList = [];

        //golden ratio-based color generation
        for (int i = 0; i < options.Length; i++) colorList.Add(Color.FromHSLA(137.5 * i, 1, 1.618 * i % 0.33 + 0.33));

        //create a RendererObject for each option
        RendererObject[] graphList = new RendererObject[options.Length];

        for (int i = 0; i < options.Length; i++)
        {
            graphList[i] = new(
                text: options[i],
                y: i + 7
            );
        }

        int selectedGraph = 0;
        int menuPosition() => -Math.Clamp(selectedGraph - renderer.TerminalHeight / 2 + 8, 0, Math.Max(options.Length + 8 - renderer.TerminalHeight, 0));

        renderer.Object = new(
            geometry: new(0, 0, renderer.TerminalWidth, renderer.TerminalHeight),
            subObjects: [
                new(
                    subObjects: [
                        new(
                            text:
                            """
                             ↑ ↓   change the selected graph
                            ENTER  display the selected graph
                            SPACE  select multiple compatible graphs
                             ESC   quit the application
                              E    export the result data
                            """,
                            externalAlignmentX:Alignment.Center,
                            y:1
                        ),
                        ..graphList,
                    ],
                    internalAlignmentX: Alignment.Center,
                    externalAlignmentX: Alignment.Center,
                    border: Borders.Rounded,
                    defaultCharacter:' ',
                    y:menuPosition()
                )
            ]
        );
        //removing empty space at the end of the list
        renderer.Object.SubObjects[0].Height--;

        //json export function for the greeter file dialogue
        JsonSerializerOptions jsonOptions = new() { WriteIndented = true };
        string tryExportFile(string filePath)
        {
            try
            {
                File.WriteAllText(filePath, resultDataManager.ToJson());
            }
            catch
            {
                return "Could not write to the selected path";
            }
            return "";
        }
        graphList[selectedGraph].ColorAreas = [new(color: colorList[selectedGraph % colorList.Count]), new(color: Colors.Black, foreground: true)];
        renderer.Update(forceRedraw: true);
        bool running = true;
        List<int> selectedList = [];

        //color getters for readability
        List<ColorArea> selectedElementColor() => [
            //only color compatible selections
            new(
                color: selectedList.Count > 0 && measurements[selectedList[0]] != measurements[selectedGraph]
                ? Colors.Gray
                : colorList[selectedGraph]
            ),
            //if selected, change foreground
            new(
                color: selectedList.Contains(selectedGraph)
                ? Colors.White
                : Colors.Black,
                foreground: true
            )
        ];

        List<ColorArea> unselectedElementColor() => [
            new(
                color: selectedList.Contains(selectedGraph)
                ? colorList[selectedGraph]
                : Colors.White,
                true
            )
        ];

        while (running)
        {
            while (Console.KeyAvailable)
            {
                switch (renderer.ReadKey().Key)
                {
                    //switch selected graph and move the menu to center the selection
                    case ConsoleKey.UpArrow:
                        graphList[selectedGraph].ColorAreas = unselectedElementColor();
                        selectedGraph = Math.Max(selectedGraph - 1, 0);
                        renderer.Object.SubObjects[0].Y = menuPosition();
                        graphList[selectedGraph].ColorAreas = selectedElementColor();
                        break;
                    case ConsoleKey.DownArrow:
                        graphList[selectedGraph].ColorAreas = unselectedElementColor();
                        selectedGraph = Math.Min(selectedGraph + 1, graphList.Length - 1);
                        renderer.Object.SubObjects[0].Y = menuPosition();
                        graphList[selectedGraph].ColorAreas = selectedElementColor();
                        break;

                    //load graph
                    case ConsoleKey.Enter:
                        List<string> names = [];
                        List<Color> colors = [];
                        List<Dictionary<Tuple<DateTime, DateTime>, double>> graphs = [];
                        //handling the selection-less edge case
                        bool singleGraph = selectedList.Count == 0;

                        if (singleGraph) selectedList.Add(selectedGraph);

                        foreach (int selected in selectedList)
                        {
                            names.Add(options[selected]);
                            colors.Add(colorList[selected]);
                            graphs.Add(graphValues[options[selected]]);
                        }

                        GraphDrawer(
                            names,
                            measurements[selectedList[0]],
                            colors,
                            times,
                            graphs
                        );

                        if (singleGraph) selectedList = [];

                        break;
                    //multi-graph selection
                    case ConsoleKey.Spacebar:
                        //if it's compatible, toggle it's presence in the list
                        if (selectedList.Count == 0 || measurements[selectedList[0]] == measurements[selectedGraph])
                            if (selectedList.Contains(selectedGraph))
                            {
                                graphList[selectedGraph].ColorAreas = [new(color: colorList[selectedGraph]), new(color: Colors.Black, foreground: true)];
                                selectedList.Remove(selectedGraph);
                            }
                            else
                            {
                                graphList[selectedGraph].ColorAreas = [new(color: colorList[selectedGraph]), new(color: Colors.White, foreground: true)];
                                selectedList.Add(selectedGraph);
                            }
                        break;

                    //quit
                    case ConsoleKey.Escape:
                        running = false;
                        break;

                    //export all data to json
                    case ConsoleKey.E:
                        bool escaped = false;
                        TextBox(
                            escaped: ref escaped,
                            text: "resultData.json",
                            title: "Input the result data file path:",
                            fileAction: tryExportFile
                        );
                        break;
                }
            }

            Thread.Sleep(50);
            if (renderer.UpdateScreenSize())
            {
                renderer.Object.Width = renderer.TerminalWidth;
                renderer.Object.Height = renderer.TerminalHeight;
                renderer.Object.SubObjects[0].Y = menuPosition();
            }
            renderer.Update();
        }
        renderer.Reset();
        Console.Clear();
        Console.SetCursorPosition(0, 0);
    }
}