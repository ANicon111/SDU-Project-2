using System.IO;
using System;
using System.Text.Json;
using System.Collections.Generic;


namespace HeatManagement;
//Daily scrum #1'

public struct Asset(string name, string imagePath, double heatCapacity, double cost, double electricityCapacity, double co2, Dictionary<string, double> additionalResources)
{
    private string name = name;
    private string imagePath = imagePath;
    private double heatCapacity = heatCapacity;
    private double cost = cost;
    private double electricityCapacity = electricityCapacity;
    private double co2 = co2;
    private Dictionary<string, double> additionalResources = additionalResources;

    public string Name { readonly get => name; set => name = value; }
    public string ImagePath { readonly get => imagePath; set => imagePath = value; }
    public double HeatCapacity { readonly get => heatCapacity; set => heatCapacity = value; }
    public double Cost { readonly get => cost; set => cost = value; }
    public double ElectricityCapacity { readonly get => electricityCapacity; set => electricityCapacity = value; }
    public double CO2 { readonly get => co2; set => co2 = value; }
    public Dictionary<string, double> AdditionalResources { readonly get => additionalResources; set => additionalResources = value; }
}

class AssetManager
{
    public List<Asset> Assets = [];
    public bool Loaded { get; private set; } = false;
    public void JsonImport(string json)
    {
        Loaded = false;
        Assets = JsonSerializer.Deserialize<List<Asset>>(json) ?? throw new("Null json result");
        Loaded = true;
    }

    public string JsonExport() => JsonSerializer.Serialize(Assets);



    public void DataAdd(string name, string imagePath, double heatCapacity, double cost, double electricityCapacity, double co2, Dictionary<string, double> additionalResources)
    {
        Assets.Add(new Asset(name, imagePath, heatCapacity, cost, electricityCapacity, co2, additionalResources));
    }
    public void DataRemove(string name)
    {
        Assets.RemoveAll(asset => asset.Name == name);
    }
}


