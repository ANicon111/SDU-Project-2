namespace HeatManagement;

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;


[method: SetsRequiredMembers]

public struct ResultData(double heat, double cost, double electricity, double co2, Dictionary<string, double> additionalResources)
{
    public required double Heat { get; set; } = heat;
    public required double Cost { get; set; } = cost;
    public required double Electricity { get; set; } = electricity;
    public required double CO2 { get; set; } = co2;
    public required Dictionary<string, double> AdditionalResources { get; set; } = additionalResources;
}

public class ResultDataManager
{
    [method: SetsRequiredMembers]
    public struct JsonData(DateTime startTime, DateTime endTime, Dictionary<string, ResultData> resultData)
    {
        public required DateTime StartTime { get; set; } = startTime;
        public required DateTime EndTime { get; set; } = endTime;
        public required Dictionary<string, ResultData> ResultData { get; set; } = resultData;
    }

    public SortedDictionary<Tuple<DateTime, DateTime>, Dictionary<string, ResultData>> ResultData;

    public ResultDataManager(string json = "[]")
    {
        ResultData = [];
        List<JsonData> jsonIntermediary = JsonSerializer.Deserialize<List<JsonData>>(json)!;
        foreach (var element in jsonIntermediary)
        {
            ResultData.Add(Tuple.Create(element.StartTime, element.EndTime), element.ResultData);
        }
    }

    public void AddData(DateTime startTime, DateTime endTime, string assetName, ResultData assetData)
    {
        if (!ResultData.ContainsKey(Tuple.Create(startTime, endTime))) ResultData[Tuple.Create(startTime, endTime)] = [];
        if (!ResultData[Tuple.Create(startTime, endTime)].ContainsKey(assetName)) ResultData[Tuple.Create(startTime, endTime)][assetName] = assetData;
    }

    public void RemoveData(DateTime startTime, DateTime endTime, string? assetName = null)
    {
        if (assetName != null)
        {
            ResultData[Tuple.Create(startTime, endTime)].Remove(assetName);
        }
        else
        {
            ResultData.Remove(Tuple.Create(startTime, endTime));
        }
    }

    public string ToJson()
    {
        List<JsonData> jsonIntermediary = [];
        foreach (var element in ResultData)
        {
            jsonIntermediary.Add(new(element.Key.Item1, element.Key.Item2, element.Value));
        }
        return JsonSerializer.Serialize(jsonIntermediary);
    }
}