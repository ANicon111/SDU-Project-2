using System;
using System.Collections.Generic;
using System.Text.Json;

namespace HeatManagement;

public struct Source(DateTime startTime, DateTime endTime, double heatDemand, double electricityPrice)
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

class SourceDataManager
{
    public List<Source> SourceData = [];
    void JsonImport(string json)
    {
        try
        {
            SourceData = JsonSerializer.Deserialize<List<Source>>(json)!;
        }
        catch
        {
            throw new Exception("Error: Failed to parse Json");
        }
    }

    string JsonExport() => JsonSerializer.Serialize(SourceData);

    public void DataAdd(DateTime startTime, DateTime endTime, double heatDemand, double electricityPrice)
    {
        SourceData.Add(new Source(startTime, endTime, heatDemand, electricityPrice));
    }
    public void DataRemove(DateTime startTime)
    {
        SourceData.RemoveAll(source => source.StartTime == startTime);
    }
}