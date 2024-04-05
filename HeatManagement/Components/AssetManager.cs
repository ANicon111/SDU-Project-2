namespace HeatManagement;

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Xml.Serialization;
using DynamicData;

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

public class AssetManager
{
    //The name is stored as the Dictionary index
    private SortedDictionary<string, Asset> assets;
    public SortedDictionary<string, Asset> Assets { get => assets; set => assets = value; }

    private AssetManager(SortedDictionary<string, Asset> assets)
    {
        this.assets = assets;
    }

    public AssetManager()
    {
        assets = [];
    }

    public static AssetManager FromJson(string json)
    {
        try
        {
            SortedDictionary<string, Asset> assets = JsonSerializer.Deserialize<SortedDictionary<string, Asset>>(json) ?? throw new("Null value");
            return new(assets);
        }
        catch
        {
            throw new("Invalid Data");
        }
    }
    public string ToJson(JsonSerializerOptions? options = null) => JsonSerializer.Serialize(assets, options);

    public static AssetManager FromCSV(string csv)
    {
        string[] rows = csv.Split(["\n\r", "\r\n", "\r", "\n"], StringSplitOptions.None);

        if (rows.Length == 0) throw new("Invalid Data");

        //check header
        if (rows[0] != "Name,DataType,ImagePath,HeatCapacity,Cost,ElectricityCapacity,CO2,AdditionalResourceName,AdditionalResourceValue")
            throw new("Invalid Data");

        SortedDictionary<string, Asset> assets = [];

        for (int i = 1; i < rows.Length; i++)
        {
            string[] columns = rows[i].Split(',');
            if (columns.Length != 9) throw new($"CSV: Invalid column count at row {i}");
            string name = columns[0].Trim().ToUpper();
            if (columns[1].Trim() == "Base")
            {
                string imagePath = columns[2];

                string temp = columns[3].Replace("\"", "");
                if (!double.TryParse(temp, CultureInfo.InvariantCulture, out double heatCapacity))
                    throw new($"CSV: Invalid HeatCapacity at row {i}");

                temp = columns[4].Replace("\"", "");
                if (!double.TryParse(temp, CultureInfo.InvariantCulture, out double cost))
                    throw new($"CSV: Invalid Cost at row {i}");

                temp = columns[5].Replace("\"", "");
                if (!double.TryParse(temp, CultureInfo.InvariantCulture, out double electricityCapacity))
                    throw new($"CSV: Invalid ElectricityCapacity at row {i}");

                temp = columns[6].Replace("\"", "");
                if (!double.TryParse(temp, CultureInfo.InvariantCulture, out double co2))
                    throw new($"CSV: Invalid CO2 at row {i}");

                if (!assets.TryAdd(name, new(imagePath, heatCapacity, cost, electricityCapacity, co2, [])))
                    throw new($"CSV: Duplicate asset name at row {i}");
            }
            else if (columns[1].Trim() == "Additional")
            {
                string additionalResourceName = columns[7].Trim().ToLower();

                string temp = columns[8].Replace("\"", "");
                if (!double.TryParse(temp, CultureInfo.InvariantCulture, out double additionalResourceValue))
                    throw new($"CSV: Invalid AdditionalResourceValue at row {i}");

                if (!assets.TryGetValue(name, out Asset asset)) throw new($"CSV: Uninitialized asset at row {i}");

                if (!asset.AdditionalResources.TryAdd(additionalResourceName, additionalResourceValue)) throw new($"CSV: Duplicate additional resource at row {i}");
            }
            else throw new($"CSV: Invalid DataType at row {i}");
        }
        return new(assets);
    }
    public string ToCSV()
    {
        //standard header
        List<string> rows = ["Name,DataType,ImagePath,HeatCapacity,Cost,ElectricityCapacity,CO2,AdditionalResourceName,AdditionalResourceValue"];

        //values
        foreach (KeyValuePair<string, Asset> asset in Assets)
        {
            rows.Add(FormattableString.Invariant($"{asset.Key},Base,{asset.Value.ImagePath},{asset.Value.HeatCapacity},{asset.Value.Cost},{asset.Value.ElectricityCapacity},{asset.Value.CO2},,"));
            foreach (KeyValuePair<string, double> additionalResource in asset.Value.AdditionalResources)
            {
                rows.Add(FormattableString.Invariant($"{asset.Key},Additional,,,,,,{additionalResource.Key},{additionalResource.Value}"));
            }
        }

        return string.Join('\n', rows);
    }

    public static AssetManager FromAnySupportedFormat(string text)
    {
        AssetManager? assetManager = null;

        try
        {
            assetManager = FromJson(text);
        }
        catch (Exception e)
        {
            if (e.Message != "Invalid Data")
                throw;
        }

        try
        {
            assetManager = FromCSV(text);
        }
        catch (Exception e)
        {
            if (e.Message != "Invalid Data")
                throw;
        }

        if (assetManager == null) throw new("Invalid Data");
        return assetManager;
    }

    public void AddAsset(string name, Asset asset)
    {
        assets?.Add(name, asset);
    }
    public void RemoveAsset(string name)
    {
        assets?.Remove(name);
    }
}