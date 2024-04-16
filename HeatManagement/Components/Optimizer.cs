using System;
using System.Collections.Generic;
using System.Linq;

namespace HeatManagement;

public static class Optimizer
{
    public enum Value
    {
        Cost,
        CO2,
        ElectricityProduction,
        ElectricityConsumption
    }
    public static void GetResult(AssetManager assets, SourceDataManager sourceData, ResultDataManager resultData, Value sortedBy)
    {
        resultData.Assets = assets.Assets;
        foreach (KeyValuePair<Tuple<DateTime, DateTime>, SourceData> dataUnit in sourceData.Data)
        {
            Dictionary<string, NewStruct> assetSortingValues = [];
            double remainingUsage = dataUnit.Value.HeatDemand;

            foreach ((string name, Asset asset) in assets.Assets!)
            {
                double costPerMWH = asset.Cost - asset.ElectricityCapacity * dataUnit.Value.ElectricityPrice / asset.HeatCapacity;
                assetSortingValues.Add(name, new(costPerMWH, asset.CO2, asset.ElectricityCapacity));
            }
            //precision error avoidance
            while (remainingUsage > 1e-8 && assetSortingValues.Count > 0)
            {
                Dictionary<string, AdditionalResource> additionalResourceUsage = [];
                string name = sortedBy switch
                {
                    Value.CO2 => assetSortingValues.MinBy(pair => pair.Value.CO2PerMWh).Key,
                    Value.ElectricityConsumption => assetSortingValues.MinBy(pair => pair.Value.ElectricityPerMWh).Key,
                    Value.ElectricityProduction => assetSortingValues.MinBy(pair => -pair.Value.ElectricityPerMWh).Key,
                    _ => assetSortingValues.MinBy(pair => pair.Value.CostPerMWh).Key,
                };
                double coveredUsage = Math.Min(assets.Assets[name].HeatCapacity, remainingUsage);
                double costPerMWH = assets.Assets[name].Cost - assets.Assets[name].ElectricityCapacity * dataUnit.Value.ElectricityPrice / assets.Assets[name].HeatCapacity;
                remainingUsage -= coveredUsage;
                foreach ((string resourceName, AdditionalResource additionalResource) in assets.Assets[name].AdditionalResources)
                {
                    if (!additionalResourceUsage.ContainsKey(resourceName)) additionalResourceUsage[resourceName] = new(0, additionalResource.Measurement);
                    additionalResourceUsage[resourceName].Value += additionalResource.Value * coveredUsage;
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
                assetSortingValues.Remove(name);
            }
        }
    }
}

internal struct NewStruct(double costPerMWh, double co2PerMWh, double electricityPerMWh)
{
    public double CostPerMWh = costPerMWh;
    public double CO2PerMWh = co2PerMWh;
    public double ElectricityPerMWh = electricityPerMWh;
}