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
        List<Color> optionColors = [Colors.Lime, Colors.Yellow, Colors.Gray, Colors.Crimson, Colors.LawnGreen, Colors.SkyBlue, Colors.BlanchedAlmond, Colors.Sienna, Colors.Purple, Colors.Goldenrod, Colors.BlueViolet, Colors.PeachPuff, Colors.Beige];
        Dictionary<string, Dictionary<Tuple<DateTime, DateTime>, double>> graphValues = new(){
            {"Cost", []},
            {"Electricity", []},
            {"CO2", []},
        };

        //get costs, electricity and co2 lists
        foreach (KeyValuePair<Tuple<DateTime, DateTime>, ResultData> result in resultDataManager.CompiledResultData)
        {
            graphValues["Cost"].Add(result.Key, result.Value.Cost);
            graphValues["Electricity"].Add(result.Key, result.Value.Electricity);
            graphValues["CO2"].Add(result.Key, result.Value.CO2);

            //get additional resource usage
            foreach (var additionalResource in result.Value.AdditionalResources)
            {
                //format resource name
                string name = additionalResource.Key[0].ToString().ToUpper() + additionalResource.Key[1..].ToLower();
                if (!options.Contains(name))
                {
                    options.Add(name);
                    graphValues.Add(name, []);
                }
                graphValues[name].Add(result.Key, additionalResource.Value);
            }
        }

        while (true)
        {
            //TODO CLI selector
            Thread.Sleep(50);
            renderer.UpdateScreenSize();
            renderer.Update(forceRedraw: true);
        }
    }
}