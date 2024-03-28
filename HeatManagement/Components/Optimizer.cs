using System;
using System.Collections.Generic;
using System.Linq;

namespace HeatManagement;

public static class Optimizer
{
    public static void GetResult(AssetManager am, SourceDataManager sdm, ResultDataManager rdm)
    {
        foreach (KeyValuePair<Tuple<DateTime, DateTime>, SourceData> dataUnit in sdm.Data!)
        {
            Dictionary<string, double> assetCostPerMWH = new();
            double remainingUsage = dataUnit.Value.HeatDemand;

            foreach ((string name, Asset asset) in am.Assets!)
            {
                double costPerMWH = asset.Cost - asset.ElectricityCapacity * dataUnit.Value.ElectricityPrice / asset.HeatCapacity;
                assetCostPerMWH.Add(name, costPerMWH);
            }
            //precision error avoidance
            while (remainingUsage > 1e-8 && assetCostPerMWH.Count > 0)
            {
                Dictionary<string, double> additionalResourceUsage = [];
                (string name, double costPerMWH) = assetCostPerMWH.MinBy(pair => pair.Value);
                double coveredUsage = Math.Min(am.Assets[name].HeatCapacity, remainingUsage);
                remainingUsage -= coveredUsage;
                foreach ((string resourceName, double resourceUsage) in am.Assets[name].AdditionalResources)
                {
                    if (!additionalResourceUsage.ContainsKey(resourceName)) additionalResourceUsage[resourceName] = 0;
                    additionalResourceUsage[resourceName] += resourceUsage * coveredUsage;
                }
                rdm.AddData(
                    dataUnit.Value.StartTime,
                    dataUnit.Value.EndTime,
                    name,
                    new(
                        coveredUsage,
                        coveredUsage * costPerMWH,
                        //assuming proportional electricity and heat production/consumption, this is the electricity delta per mwh of heat produced 
                        am.Assets[name].ElectricityCapacity / am.Assets[name].HeatCapacity * coveredUsage,
                        am.Assets[name].CO2 * coveredUsage,
                        additionalResourceUsage
                    )
                );
                assetCostPerMWH.Remove(name);
            }
        }
    }
}