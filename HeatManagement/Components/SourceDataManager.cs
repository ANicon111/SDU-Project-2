namespace HeatManagement;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;


public struct SourceData(DateTime startTime, DateTime endTime, double heatDemand, double electricityPrice)
{
    private DateTime startTime = startTime;
    private DateTime endTime = endTime;
    private double heatDemand = heatDemand;
    private double electricityPrice = electricityPrice;

    public DateTime StartTime { readonly get => startTime; set => startTime = value; }
    public DateTime EndTime { readonly get => endTime; set => endTime = value; }
    public double HeatDemand { readonly get => heatDemand; set => heatDemand = value; }
    public double ElectricityPrice { readonly get => electricityPrice; set => electricityPrice = value; }
}

public class SourceDataManager(string filePath = "sourceData.json", bool generateFileIfNotExists = false, bool overwriteFile = false)
{
    readonly string FilePath = filePath;

    private List<SourceData>? data = JsonSerializer.Deserialize<List<SourceData>>(
        !File.Exists(filePath) && generateFileIfNotExists || overwriteFile
        ? "[]"
        : File.ReadAllText(filePath)
    );

    public List<SourceData>? Data { get => data; set => data = value; }

    public void StoreJson() => File.WriteAllText(FilePath, JsonSerializer.Serialize(data));
    public void AddData(SourceData sourceData)
    {
        data?.Add(sourceData);
    }
    public void RemoveData(SourceData sourceData)
    {
        data?.Remove(sourceData);
    }
}