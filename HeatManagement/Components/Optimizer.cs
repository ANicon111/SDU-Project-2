using System;
using System.Collections.Generic;
using System.Linq;

namespace HeatManagement;

public static class Optimizer
{
    public static void GetResult(AssetManager assets, SourceDataManager sourceData, ResultDataManager resultData)
    {
        resultData.Assets = assets.Assets;
        foreach (KeyValuePair<Tuple<DateTime, DateTime>, SourceData> dataUnit in sourceData.Data)
        {
            Dictionary<string, double> assetCostPerMWH = [];
            double remainingUsage = dataUnit.Value.HeatDemand;

            foreach ((string name, Asset asset) in assets.Assets!)
            {
                double costPerMWH = asset.Cost - asset.ElectricityCapacity * dataUnit.Value.ElectricityPrice / asset.HeatCapacity;
                assetCostPerMWH.Add(name, costPerMWH);
            }
            //precision error avoidance
            while (remainingUsage > 1e-8 && assetCostPerMWH.Count > 0)
            {
                Dictionary<string, double> additionalResourceUsage = [];
                (string name, double costPerMWH) = assetCostPerMWH.MinBy(pair => pair.Value);
                double coveredUsage = Math.Min(assets.Assets[name].HeatCapacity, remainingUsage);
                remainingUsage -= coveredUsage;
                foreach ((string resourceName, double resourceUsage) in assets.Assets[name].AdditionalResources)
                {
                    if (!additionalResourceUsage.ContainsKey(resourceName)) additionalResourceUsage[resourceName] = 0;
                    additionalResourceUsage[resourceName] -= resourceUsage * coveredUsage;
                }
                resultData.AddData(
                    dataUnit.Value.StartTime,
                    dataUnit.Value.EndTime,
                    name,
                    new(
                        coveredUsage,
                        coveredUsage * costPerMWH,
                        //assuming proportional electricity and heat production/consumption, this is the electricity delta per mwh of heat produced 
                        assets.Assets[name].ElectricityCapacity / assets.Assets[name].HeatCapacity * coveredUsage,
                        assets.Assets[name].CO2 * coveredUsage,
                        additionalResourceUsage
                    )
                );
                assetCostPerMWH.Remove(name);
            }
        }
    }
}