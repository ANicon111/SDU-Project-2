using System;
using System.Collections.Generic;
using System.Linq;
namespace HeatManagement;

partial class Optimizer(AssetManager assetManager, SourceDataManager sourceDataManager, ResultDataManager resultDataManager)
{
    private AssetManager AssetManager = assetManager;
    private SourceDataManager SourceDataManager = sourceDataManager;
    private ResultDataManager ResultDataManager = resultDataManager;

    public enum SortBy
    {
        Cost,
        CO2,
        ElectricityConsumption,
        ElectricityProduction,
        HeatCapacity,
    }

    private class AssetWithCostPerMWh(Asset asset, double costPerMWh, double co2PerMWh, double electricityPerMWh, double peakHeat)
    {
        public Asset Asset { get; } = asset;
        public double CostPerMWh { get; } = costPerMWh;
        public double CO2PerMWh { get; } = co2PerMWh;
        public double ElectricityPerMWh { get; } = electricityPerMWh;
        public double PeakHeat { get; } = peakHeat;

    }

    public void Optimize(SortBy sortValue)
    {
        //heatCapacity * asset.cost - electricityCapacity * electricityPrice
        List<Asset> assets = AssetManager.Assets;
        List<Source> sourceData = SourceDataManager.SourceData;

        foreach (Source source in sourceData)
        {
            List<AssetWithCostPerMWh> assetsWithSorts = [];
            foreach (Asset asset in assets)
            {
                double costPerMWh = (asset.HeatCapacity * asset.Cost - asset.ElectricityCapacity * source.ElectricityPrice) / asset.HeatCapacity;
                double co2PerMWh = asset.CO2;
                double electricityPerMWh = asset.ElectricityCapacity / asset.HeatCapacity;
                double peakHeat = asset.HeatCapacity;

                assetsWithSorts.Add(new(asset, costPerMWh, co2PerMWh, electricityPerMWh, peakHeat));
            }

            assetsWithSorts = [.. sortValue switch{
                SortBy.Cost=>assetsWithSorts.OrderBy((asset) => asset.CostPerMWh),
                SortBy.CO2=>assetsWithSorts.OrderBy((asset) => asset.CO2PerMWh).ThenBy((asset) => asset.CostPerMWh),
                SortBy.ElectricityConsumption=>assetsWithSorts.OrderBy((asset) => asset.ElectricityPerMWh).ThenBy((asset) => asset.CostPerMWh),
                SortBy.ElectricityProduction=>assetsWithSorts.OrderBy((asset) => -asset.ElectricityPerMWh).ThenBy((asset) => asset.CostPerMWh),
                SortBy.HeatCapacity=>assetsWithSorts.OrderByDescending((asset) => asset.PeakHeat).ThenBy((asset) => asset.CostPerMWh),

                _ => throw new("Invalid sorting option."), // It should be impossible to reach this exception, but should prevent future programming mistakes 
            }];
            double currentDemand = source.HeatDemand;

            List<Result> results = [];
            foreach (AssetWithCostPerMWh asset in assetsWithSorts)
            {
                double producedHeat = Math.Min(currentDemand, asset.Asset.HeatCapacity);
                currentDemand -= producedHeat;
                double assetPercentageUsage = producedHeat / asset.Asset.HeatCapacity;
                double consumedElectricity = asset.Asset.ElectricityCapacity * assetPercentageUsage;
                double cost = (asset.Asset.HeatCapacity * asset.Asset.Cost - asset.Asset.ElectricityCapacity * source.ElectricityPrice) * assetPercentageUsage;
                double producedCO2 = asset.Asset.CO2 * producedHeat;
                Dictionary<string, double> additionalResourceUsage = [];

                foreach (KeyValuePair<string, double> resource in asset.Asset.AdditionalResources)
                {
                    additionalResourceUsage.Add(resource.Key, resource.Value * producedHeat);
                }
                results.Add(new(
                    asset: asset.Asset.Name,
                    imagePath: asset.Asset.ImagePath,
                    producedHeat: producedHeat,
                    consumedElectricity: consumedElectricity,
                    cost: cost,
                    producedCO2: producedCO2,
                    additionalResources: additionalResourceUsage
                ));

                if (source.HeatDemand == 0) break;
            }

            DateTime startTime = source.StartTime;
            DateTime endTime = source.EndTime;
            ResultDataManager.DataAdd(new(
                startTime: startTime,
                endTime: endTime,
                results: results
            ));
        }
    }
}
