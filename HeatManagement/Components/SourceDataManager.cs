namespace HeatManagement;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;

[method: SetsRequiredMembers]
public struct SourceData(DateTime startTime, DateTime endTime, double heatDemand, double electricityPrice)
{
    public required DateTime StartTime { get; set; } = startTime;
    public required DateTime EndTime { get; set; } = endTime;
    public required double HeatDemand { get; set; } = heatDemand;
    public required double ElectricityPrice { get; set; } = electricityPrice;
}

public class SourceDataManager(string json = "[]")
{
    private List<SourceData>? data = JsonSerializer.Deserialize<List<SourceData>>(json);

    public List<SourceData>? Data { get => data; set => data = value; }

    public void AddData(SourceData sourceData)
    {
        data?.Add(sourceData);
    }
    public void RemoveData(DateTime startTime, DateTime endTime)
    {
        data?.RemoveAll((val) => val.StartTime == startTime && val.EndTime == endTime);
    }
    public string ToJson(JsonSerializerOptions? options = null) => JsonSerializer.Serialize(data, options);
}