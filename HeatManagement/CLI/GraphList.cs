using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using AnsiRenderer;

namespace HeatManagement;

static partial class CLI
{
    public static void RunGraphList(AssetManager assetManager, SourceDataManager sourceDataManager)
    {

        ResultDataManager resultDataManager = new();
        Optimizer.GetResult(assetManager, sourceDataManager, resultDataManager);
        List<string> options = ["Cost", "Electricity", "CO2"];
        List<Tuple<DateTime, DateTime>> times = [.. resultDataManager.CompiledResultData.Keys];
        List<Color> optionColors = [Colors.Lime, Colors.Yellow, Colors.Gray, Colors.Crimson, Colors.LawnGreen, Colors.SkyBlue, Colors.BlanchedAlmond, Colors.Sienna, Colors.Purple, Colors.Goldenrod, Colors.BlueViolet, Colors.PeachPuff];
        List<string> optionUnitOfMeasurements = ["dkk", "MWh", "kg"];
        Dictionary<string, Dictionary<Tuple<DateTime, DateTime>, double>> graphValues = new(){
            {"Cost", []},
            {"Electricity", []},
            {"CO2", []},
        };

        //get costs, electricity and co2 lists
        foreach (KeyValuePair<Tuple<DateTime, DateTime>, ResultData> result in resultDataManager.CompiledResultData)
        {
            graphValues["Cost"][result.Key] = result.Value.Cost;
            graphValues["Electricity"][result.Key] = result.Value.Electricity;
            graphValues["CO2"][result.Key] = result.Value.CO2;

            //get additional resource usage
            foreach (KeyValuePair<string, double> additionalResource in result.Value.AdditionalResources)
            {
                //format resource name
                string name = additionalResource.Key[0].ToString().ToUpper() + additionalResource.Key[1..].ToLower();
                if (!options.Contains(name))
                {
                    options.Add(name);
                    graphValues.Add(name, []);
                    optionUnitOfMeasurements.Add("MWh");
                }
                graphValues[name][result.Key] = additionalResource.Value;
            }
        }

        //get individual asset usage stats
        foreach (KeyValuePair<Tuple<DateTime, DateTime>, Dictionary<string, ResultData>> result in resultDataManager.RawResultData)
        {
            foreach (KeyValuePair<string, ResultData> asset in result.Value)
            {
                //format asset name
                string assetName = asset.Key.ToUpper();

                //add asset statistics; Cost, Electricity and CO2 are hard-coded,
                //so linking them with a single if is simply more efficient
                if (!options.Contains($"{assetName}-Cost"))
                {
                    options.Add($"{assetName}-Cost");
                    graphValues.Add($"{assetName}-Cost", []);
                    optionUnitOfMeasurements.Add("dkk");
                    options.Add($"{assetName}-Electricity");
                    graphValues.Add($"{assetName}-Electricity", []);
                    optionUnitOfMeasurements.Add("MWh");
                    options.Add($"{assetName}-CO2");
                    graphValues.Add($"{assetName}-CO2", []);
                    optionUnitOfMeasurements.Add("kg");
                }
                graphValues[$"{assetName}-Cost"][result.Key] = asset.Value.Cost;
                graphValues[$"{assetName}-Electricity"][result.Key] = asset.Value.Electricity;
                graphValues[$"{assetName}-CO2"][result.Key] = asset.Value.CO2;
                //get additional resource usage
                foreach (KeyValuePair<string, double> additionalResource in asset.Value.AdditionalResources)
                {
                    //format resource name
                    string resourceName = additionalResource.Key[0].ToString().ToUpper() + additionalResource.Key[1..].ToLower();
                    if (!options.Contains($"{assetName}-{resourceName}"))
                    {
                        options.Add($"{assetName}-{resourceName}");
                        graphValues.Add($"{assetName}-{resourceName}", []);
                        optionUnitOfMeasurements.Add("MWh");
                    }
                    graphValues[$"{assetName}-{resourceName}"][result.Key] = additionalResource.Value;
                }
            }

        }

        //create RendererObject list for the options
        RendererObject[] graphs = new RendererObject[options.Count];

        for (int i = 0; i < options.Count; i++)
        {
            graphs[i] = new(
                text: options[i],
                y: i + 1

            );
        }

        int selectedGraph = 0;
        renderer.Object = new(
            geometry: new(0, 0, renderer.TerminalWidth, renderer.TerminalHeight),
            subObjects: [
                new(
                    subObjects: graphs,
                    internalAlignmentX: Alignment.Center,
                    externalAlignmentX: Alignment.Center,
                    border: Borders.Rounded
                )
            ]
        );
        renderer.Object.SubObjects[0].Height--;

        graphs[selectedGraph].ColorAreas = [new(color: optionColors[selectedGraph % optionColors.Count]), new(color: Colors.Black, foreground: true)];
        renderer.Update(forceRedraw: true);
        while (true)
        {
            if (Console.KeyAvailable)
            {
                switch (renderer.ReadKey().Key)
                {
                    case ConsoleKey.UpArrow:
                        graphs[selectedGraph].ColorAreas = [];
                        selectedGraph = Math.Max(selectedGraph - 1, 0);
                        renderer.Object.SubObjects[0].Y = -Math.Max(0, selectedGraph - renderer.TerminalHeight + 3);
                        graphs[selectedGraph].ColorAreas =
                            [new(color: optionColors[selectedGraph % optionColors.Count]), new(color: Colors.Black, foreground: true)];
                        break;
                    case ConsoleKey.DownArrow:
                        graphs[selectedGraph].ColorAreas = [];
                        selectedGraph = Math.Min(selectedGraph + 1, graphs.Length - 1);
                        renderer.Object.SubObjects[0].Y = -Math.Max(0, selectedGraph - renderer.TerminalHeight + 3);
                        graphs[selectedGraph].ColorAreas =
                            [new(color: optionColors[selectedGraph % optionColors.Count]), new(color: Colors.Black, foreground: true)];
                        break;
                    case ConsoleKey.Enter:
                        GraphDrawer(
                            options[selectedGraph],
                            optionUnitOfMeasurements[selectedGraph],
                            optionColors[selectedGraph % optionColors.Count],
                            times,
                            graphValues[options[selectedGraph]]
                        );
                        break;
                }
                while (Console.KeyAvailable) renderer.ReadKey();
            }
            Thread.Sleep(50);
            if (renderer.UpdateScreenSize())
            {
                renderer.Object.Width = renderer.TerminalWidth;
                renderer.Object.Height = renderer.TerminalHeight;
                renderer.Object.SubObjects[0].Y = -Math.Max(0, selectedGraph - renderer.TerminalHeight + 3);
            }
            renderer.Update();
        }
    }
}