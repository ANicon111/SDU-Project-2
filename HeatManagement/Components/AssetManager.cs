namespace HeatManagement;

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text.Json;

public class AdditionalResource(double value, string measurement)
{
    public string Measurement { get; set; } = measurement;
    public double Value { get; set; } = value;
}

[method: SetsRequiredMembers]
public struct Asset(string imagePath, double heatCapacity, double cost, double electricityCapacity, double co2, Dictionary<string, AdditionalResource> additionalResources)
{
    public required string ImagePath { get; set; } = imagePath;
    public required double HeatCapacity { get; set; } = heatCapacity;
    public required double Cost { get; set; } = cost;
    public required double ElectricityCapacity { get; set; } = electricityCapacity;
    public required double CO2 { get; set; } = co2;
    public required Dictionary<string, AdditionalResource> AdditionalResources { get; set; } = additionalResources;
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
        if (rows[0] != "Name,DataType,ImagePath,HeatCapacity,Cost,ElectricityCapacity,CO2,AdditionalResourceName,AdditionalResourceValue,AdditionalResourceMeasurement")
            throw new("Invalid Data");

        SortedDictionary<string, Asset> assets = [];

        for (int i = 1; i < rows.Length; i++)
        {
            //skip empty rows
            if (string.IsNullOrWhiteSpace(rows[i])) continue;

            //CSV comma splitter
            List<string> columns = [];
            int l = 0, r = 0;
            bool inQuote = false;
            while (r < rows[i].Length)
            {
                if (rows[i][r] == '"') inQuote = !inQuote;
                if (!inQuote && rows[i][r] == ',')
                {
                    columns.Add(rows[i][l..r]);
                    l = r + 1;
                }
                r++;
            }
            columns.Add(rows[i][l..r]);

            if (columns.Count != 10) throw new($"CSV: Invalid column count at row {i}");

            //remove extra quotes from columns containing commas
            for (int j = 0; j < columns.Count; j++)
            {
                try
                {
                    if (columns[j].Contains(',')) columns[j] = columns[j].Replace("\"\"", "\"")[1..^1];
                }
                catch
                {
                    throw new($"CSV: Invalid comma escape at row {i}, column {j + 1}");
                }
            }

            string name = columns[0].Trim().ToUpper();
            if (columns[1].Trim() == "Base")
            {
                string imagePath = columns[2];

                string temp = columns[3].Replace(',', '.');
                if (!double.TryParse(temp, CultureInfo.InvariantCulture, out double heatCapacity))
                    throw new($"CSV: Invalid HeatCapacity at row {i}");

                temp = columns[4].Replace(',', '.');
                if (!double.TryParse(temp, CultureInfo.InvariantCulture, out double cost))
                    throw new($"CSV: Invalid Cost at row {i}");

                temp = columns[5].Replace(',', '.');
                if (!double.TryParse(temp, CultureInfo.InvariantCulture, out double electricityCapacity))
                    throw new($"CSV: Invalid ElectricityCapacity at row {i}");

                temp = columns[6].Replace(',', '.');
                if (!double.TryParse(temp, CultureInfo.InvariantCulture, out double co2))
                    throw new($"CSV: Invalid CO2 at row {i}");

                if (!assets.TryAdd(name, new(imagePath, heatCapacity, cost, electricityCapacity, co2, [])))
                    throw new($"CSV: Duplicate asset name at row {i}");
            }
            else if (columns[1].Trim() == "Additional")
            {
                string additionalResourceName = columns[7].Trim().ToLower();

                string temp = columns[8].Replace(',', '.');
                if (!double.TryParse(temp, CultureInfo.InvariantCulture, out double additionalResourceValue))
                    throw new($"CSV: Invalid AdditionalResourceValue at row {i}");

                if (!assets.TryGetValue(name, out Asset asset)) throw new($"CSV: Uninitialized asset at row {i}");
                string AdditionalResourceMeasurement = columns[9].Trim();

                if (!asset.AdditionalResources.TryAdd(additionalResourceName, new(additionalResourceValue, AdditionalResourceMeasurement))) throw new($"CSV: Duplicate additional resource at row {i}");
            }
            else throw new($"CSV: Invalid DataType at row {i}");
        }
        return new(assets);
    }
    public string ToCSV()
    {
        //standard header
        List<string[]> table = [["Name", "DataType", "ImagePath", "HeatCapacity", "Cost", "ElectricityCapacity", "CO2", "AdditionalResourceName", "AdditionalResourceValue", "AdditionalResourceMeasurement"]];

        //values
        foreach (KeyValuePair<string, Asset> asset in Assets)
        {
            table.Add([$"{asset.Key}", $"Base", $"{asset.Value.ImagePath}", $"{asset.Value.HeatCapacity}", $"{asset.Value.Cost}", $"{asset.Value.ElectricityCapacity}", $"{asset.Value.CO2}", $"", $"", $""]);
            foreach (KeyValuePair<string, AdditionalResource> additionalResource in asset.Value.AdditionalResources)
            {
                table.Add([$"{asset.Key}", $"Additional", $"", $"", $"", $"", $"", $"{additionalResource.Key}", $"{additionalResource.Value.Value}", $"{additionalResource.Value.Measurement}"]);
            }
        }
        return CSVUtils.TableToString(table);
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