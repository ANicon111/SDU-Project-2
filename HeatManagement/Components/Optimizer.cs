using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace HeatManagement;

public static class Optimizer
{
    public static void GetResult(AssetManager am, SourceDataManager sdm, ResultDataManager rdm)
    {
        foreach (SourceData dataUnit in sdm.Data!)
        {
            Dictionary<string, double> assetCostPerMWH = new();
            double cost = 0;
            double electricityUsage = 0;
            double co2 = 0;
            double remainingUsage = dataUnit.HeatDemand;
            Dictionary<string, double> assetUsage = [];
            Dictionary<string, double> additionalResourceUsage = [];

            foreach ((string name, Asset asset) in am.Assets!)
            {
                double costPerMWH = asset.Cost - asset.ElectricityCapacity * dataUnit.ElectricityPrice / asset.HeatCapacity;
                assetCostPerMWH.Add(name, costPerMWH);
            }
            //precision error avoidance
            while (remainingUsage > 1e-8 && assetCostPerMWH.Count > 0)
            {
                (string name, double costPerMWH) = assetCostPerMWH.MinBy(pair => pair.Value);
                double coveredUsage = Math.Min(am.Assets[name].HeatCapacity, remainingUsage);
                cost += coveredUsage * costPerMWH;
                //assuming proportional electricity and heat production/consumption, this is the electricity delta per mwh of heat produced 
                electricityUsage += am.Assets[name].ElectricityCapacity / am.Assets[name].HeatCapacity * coveredUsage;
                co2 += am.Assets[name].CO2 * coveredUsage;
                remainingUsage -= coveredUsage;
                assetUsage.Add(name, coveredUsage);
                foreach ((string resourceName, double resourceUsage) in am.Assets[name].AdditionalResources)
                {
                    if (!additionalResourceUsage.ContainsKey(resourceName)) additionalResourceUsage[resourceName] = 0;
                    additionalResourceUsage[resourceName] += resourceUsage * coveredUsage;
                }
                assetCostPerMWH.Remove(name);
            }
            rdm.AddData(new(dataUnit.StartTime, dataUnit.EndTime, cost, electricityUsage, co2, remainingUsage > 1e-8 ? remainingUsage : 0, assetUsage, additionalResourceUsage));
        }
        rdm.StoreJson();
    }
}