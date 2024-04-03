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

public class SourceDataManager
{
    private SortedDictionary<Tuple<DateTime, DateTime>, SourceData> data;

    public SourceDataManager(string? json = null)
    {
        data = [];
        if (json != null)
        {
            List<SourceData> jsonIntermediary = JsonSerializer.Deserialize<List<SourceData>>(json) ?? throw new("Null value");
            foreach (SourceData dataElement in jsonIntermediary)
            {
                data.Add(Tuple.Create(dataElement.StartTime, dataElement.EndTime), dataElement);
            }
        }
    }

    public SortedDictionary<Tuple<DateTime, DateTime>, SourceData> Data { get => data; set => data = value; }

    public void AddData(SourceData sourceData)
    {
        data?.Add(Tuple.Create(sourceData.StartTime, sourceData.EndTime), sourceData);
    }
    public void RemoveData(DateTime startTime, DateTime endTime)
    {
        data?.Remove(Tuple.Create(startTime, endTime));
    }
    public string ToJson(JsonSerializerOptions? options = null)
    {
        List<SourceData> jsonIntermediary = [];
        foreach (var dataElement in data!)
        {
            jsonIntermediary.Add(dataElement.Value);
        }
        return JsonSerializer.Serialize(jsonIntermediary, options);
    }
}