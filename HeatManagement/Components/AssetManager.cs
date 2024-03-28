namespace HeatManagement;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;

[method: SetsRequiredMembers]
public struct Asset(string imagePath, double heatCapacity, double cost, double electricityCapacity, double co2, Dictionary<string, double> additionalResources)
{
    public required string ImagePath { get; set; } = imagePath;
    public required double HeatCapacity { get; set; } = heatCapacity;
    public required double Cost { get; set; } = cost;
    public required double ElectricityCapacity { get; set; } = electricityCapacity;
    public required double CO2 { get; set; } = co2;
    public required Dictionary<string, double> AdditionalResources { get; set; } = additionalResources;
}

public class AssetManager(string json = "{}")
{
    //The name is stored as the Dictionary index
    private SortedDictionary<string, Asset>? assets = JsonSerializer.Deserialize<SortedDictionary<string, Asset>>(json);

    public SortedDictionary<string, Asset>? Assets { get => assets; set => assets = value; }

    public void AddAsset(string name, Asset asset)
    {
        assets?.Add(name, asset);
    }
    public void RemoveAsset(string name)
    {
        assets?.Remove(name);
    }
    public string ToJson(JsonSerializerOptions? options = null) => JsonSerializer.Serialize(assets, options);
}