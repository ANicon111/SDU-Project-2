using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia.Controls;

namespace HeatManagement;

partial class Optimizer
{
    private class AssetWithCostPerMWh(Asset asset, double costPerMWh)
    {
        public Asset Asset { get; } = asset;
        public double CostPerMWh { get; } = costPerMWh;
    }

    public void Optimize()
    {
        //heatCapacity * asset.cost - electricityCapacity * electricityPrice
        List<Asset> assets = Assetmanager.Assets;
        List<Source> sourceData = SourceDataManager.SourceData;

        foreach (Source source in sourceData)
        {
            List<AssetWithCostPerMWh> assetsWithCostPerMWh = [];
            foreach (Asset asset in assets)
            {
                double costPerMWh = (asset.HeatCapacity * asset.Cost - asset.ElectricityCapacity * source.ElectricityPrice) / asset.HeatCapacity;

                assetsWithCostPerMWh.Add(new(asset, costPerMWh));
            }
            assetsWithCostPerMWh = [.. assetsWithCostPerMWh.OrderBy((asset) => asset.CostPerMWh)];

            List<Result> results = [];
            foreach (var asset in assetsWithCostPerMWh)
            {
                var producedHeat = Math.Min(source.HeatDemand, asset.Asset.HeatCapacity);
                var assetPercentageUsage = producedHeat / asset.Asset.HeatCapacity;
                var consumedElectricity = asset.Asset.ElectricityCapacity * assetPercentageUsage;
                var cost = (asset.Asset.HeatCapacity * asset.Asset.Cost - asset.Asset.ElectricityCapacity * source.ElectricityPrice) * assetPercentageUsage;
                var producedCO2 = asset.Asset.CO2 * producedHeat;
                Dictionary<string, double> additionalResourceUsage = [];

                foreach (var resource in asset.Asset.AdditionalResources)
                {
                    additionalResourceUsage.Add(resource.Key, resource.Value * producedHeat);
                }
                results.Add(new(
                    unit: asset.Asset.Name,
                    producedHeat: producedHeat,
                    consumedElectricity: consumedElectricity,
                    cost: cost,
                    producedCO2: producedCO2,
                    additionalResources: additionalResourceUsage
                ));

                if (source.HeatDemand == 0) break;
            }

            var startTime = source.StartTime;
            var endTime = source.EndTime;
            ResultDataManager.DataAdd(new(
                startTime: startTime,
                endTime: endTime,
                results: results
            ));
        }
    }

}