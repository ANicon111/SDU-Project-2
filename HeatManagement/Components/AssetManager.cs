namespace HeatManagement;
using System.Collections.Generic;
using System.Text.Json;

public struct Asset(
    string imagePath,
    double heatCapacity,
    double cost,
    double electricityCapacity,
    double co2,
    Dictionary<string, double> additionalResources
)
{
    private string imagePath = imagePath;
    private double heatCapacity = heatCapacity;
    private double cost = cost;
    private double electricityCapacity = electricityCapacity;
    private double co2 = co2;
    private Dictionary<string, double> additionalResources = additionalResources;

    public string ImagePath { readonly get => imagePath; set => imagePath = value; }
    public double HeatCapacity { readonly get => heatCapacity; set => heatCapacity = value; }
    public double Cost { readonly get => cost; set => cost = value; }
    public double ElectricityCapacity { readonly get => electricityCapacity; set => electricityCapacity = value; }
    public double CO2 { readonly get => co2; set => co2 = value; }
    public Dictionary<string, double> AdditionalResources { readonly get => additionalResources; set => additionalResources = value; }
}

public class AssetManager(string json = "{}")
{
    //The name is stored as the Dictionary index
    private Dictionary<string, Asset>? assets = JsonSerializer.Deserialize<Dictionary<string, Asset>>(json);

    public Dictionary<string, Asset>? Assets { get => assets; set => assets = value; }

    public void AddAsset(string name, Asset asset)
    {
        assets?.Add(name, asset);
    }
    public void RemoveAsset(string name)
    {
        assets?.Remove(name);
    }
    public string ToJson() => JsonSerializer.Serialize(assets);
}