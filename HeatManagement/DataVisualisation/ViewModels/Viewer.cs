using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace HeatManagement.ViewModels;
class ViewerViewModel : ViewModelBase
{
    Dictionary<string, List<double>> GraphList;
    public ViewerViewModel(ResultDataManager resultDataManager)
    {
        GraphList = new Dictionary<string, List<double>>();

        GraphList = new(){
            {"Cost",new(new double[resultDataManager.TimeResults.Count])},
            { "Electricity",new(new double[resultDataManager.TimeResults.Count])},
            { "CO2",new(new double[resultDataManager.TimeResults.Count])},
            { "Heat",new(new double[resultDataManager.TimeResults.Count])}

    };

        for (int i = 0; i < resultDataManager.TimeResults.Count; i++)
        {
            UnitResults time = resultDataManager.TimeResults[i];
            DateTime startTime = time.StartTime;
            DateTime endTime = time.EndTime;
            GraphList["Cost"][i] = 0;
            GraphList["Electricity"][i] = 0;
            GraphList["CO2"][i] = 0;
            GraphList["Heat"][i] = 0;
            foreach (Result asset in time.Results)
            {
                string assetName = asset.Asset;
                double producedHeat = asset.ProducedHeat;
                double consumedElectricity = asset.ConsumedElectricity;
                double cost = asset.Cost;
                double producedCO2 = asset.ProducedCO2;
                GraphList["Cost"][i] += cost;
                GraphList["Electricity"][i] += consumedElectricity;
                GraphList["CO2"][i] += producedCO2;
                GraphList["Heat"][i] += producedHeat;
                if (!GraphList.ContainsKey($"{assetName}-Cost"))

                {
                    GraphList[$"{assetName}-Cost"] = new(new double[resultDataManager.TimeResults.Count]);
                    GraphList[$"{assetName}-Electricity"] = new(new double[resultDataManager.TimeResults.Count]);
                    GraphList[$"{assetName}-CO2"] = new(new double[resultDataManager.TimeResults.Count]);
                    GraphList[$"{assetName}-Heat"] = new(new double[resultDataManager.TimeResults.Count]);
                }
                GraphList[$"{assetName}-Cost"][i] = cost;
                GraphList[$"{assetName}-Electricity"][i] = consumedElectricity;
                GraphList[$"{assetName}-CO2"][i] = producedCO2;
                GraphList[$"{assetName}-Heat"][i] = producedHeat;
            }
            // File.WriteAllText("debug", JsonSerializer.Serialize(GraphList));
        }
    }
}