namespace HeatManagement;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;


public struct ResultData(
    DateTime startTime,
    DateTime endTime,
    double cost,
    double electricityUsage,
    double co2,
    double uncoveredUsage,
    Dictionary<string, double> assetUsage,
    Dictionary<string, double> additionalResourceUsage
)
{
    private DateTime startTime = startTime;
    private DateTime endTime = endTime;
    private double cost = cost;
    private double electricityUsage = electricityUsage;
    private double uncoveredUsage = uncoveredUsage;
    private double co2 = co2;
    private Dictionary<string, double> additionalResources = additionalResourceUsage;
    private Dictionary<string, double> assetUsage = assetUsage;

    public DateTime StartTime { readonly get => startTime; set => startTime = value; }
    public DateTime EndTime { readonly get => endTime; set => endTime = value; }
    public double Cost { readonly get => cost; set => cost = value; }
    public double ElectricityUsage { readonly get => electricityUsage; set => electricityUsage = value; }
    public double UncoveredUsage { readonly get => uncoveredUsage; set => uncoveredUsage = value; }
    public double CO2 { readonly get => co2; set => co2 = value; }
    public Dictionary<string, double> AdditionalResources { readonly get => additionalResources; set => additionalResources = value; }
    public Dictionary<string, double> AssetUsage { readonly get => assetUsage; set => assetUsage = value; }
}

public class ResultDataManager(string filePath = "resultData.json", bool generateFileIfNotExists = false, bool overwriteFile = false)
{
    readonly string FilePath = filePath;

    private List<ResultData>? data = JsonSerializer.Deserialize<List<ResultData>>(
        !File.Exists(filePath) && generateFileIfNotExists || overwriteFile
        ? "[]"
        : File.ReadAllText(filePath)
    );

    public List<ResultData>? Data { get => data; set => data = value; }

    public void StoreJson() => File.WriteAllText(FilePath, JsonSerializer.Serialize(data, new JsonSerializerOptions
    {
        NumberHandling = System.Text.Json.Serialization.JsonNumberHandling.AllowNamedFloatingPointLiterals
    }));
    public void AddData(ResultData resultData)
    {
        data?.Add(resultData);
    }
    public void RemoveData(ResultData sourceData)
    {
        data?.Remove(sourceData);
    }
}