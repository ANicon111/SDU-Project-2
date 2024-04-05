namespace HeatManagement;

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;


[method: SetsRequiredMembers]

public struct ResultData(double heat, double cost, double electricity, double co2, Dictionary<string, AdditionalResource> additionalResources)
{
    public required double Heat { get; set; } = heat;
    public required double Cost { get; set; } = cost;
    public required double Electricity { get; set; } = electricity;
    public required double CO2 { get; set; } = co2;
    public required Dictionary<string, AdditionalResource> AdditionalResources { get; set; } = additionalResources;
}

public class ResultDataManager
{
    [method: SetsRequiredMembers]
    public struct JsonData(SortedDictionary<string, Asset> assets)
    {
        [method: SetsRequiredMembers]
        public struct JsonElement(DateTime startTime, DateTime endTime, Dictionary<string, ResultData> resultData)
        {
            public required DateTime StartTime { get; set; } = startTime;
            public required DateTime EndTime { get; set; } = endTime;
            public required Dictionary<string, ResultData> ResultData { get; set; } = resultData;
        }
        public required List<JsonElement> Data { get; set; } = [];
        public required SortedDictionary<string, Asset> Assets { get; set; } = assets;
    }


    public SortedDictionary<string, Asset> Assets;
    public SortedDictionary<Tuple<DateTime, DateTime>, Dictionary<string, ResultData>> Data;

    public ResultDataManager(string? json = null)
    {
        Data = [];
        Assets = [];
        if (json != null)
        {
            JsonData jsonIntermediary = JsonSerializer.Deserialize<JsonData>(json)!;
            Assets = jsonIntermediary.Assets;
            foreach (JsonData.JsonElement element in jsonIntermediary.Data)
            {
                Data.Add(Tuple.Create(element.StartTime, element.EndTime), element.ResultData);
            }
        }
    }

    public void AddData(DateTime startTime, DateTime endTime, string assetName, ResultData assetData)
    {
        if (!Data.ContainsKey(Tuple.Create(startTime, endTime))) Data[Tuple.Create(startTime, endTime)] = [];
        if (!Data[Tuple.Create(startTime, endTime)].ContainsKey(assetName)) Data[Tuple.Create(startTime, endTime)][assetName] = assetData;
    }

    public void RemoveData(DateTime startTime, DateTime endTime, string? assetName = null)
    {
        if (assetName != null)
        {
            Data[Tuple.Create(startTime, endTime)].Remove(assetName);
        }
        else
        {
            Data.Remove(Tuple.Create(startTime, endTime));
        }
    }

    public string ToJson(JsonSerializerOptions? options = null)
    {
        JsonData jsonIntermediary = new(Assets);
        foreach (KeyValuePair<Tuple<DateTime, DateTime>, Dictionary<string, ResultData>> element in Data)
        {
            jsonIntermediary.Data.Add(new(element.Key.Item1, element.Key.Item2, element.Value));
        }
        return JsonSerializer.Serialize(jsonIntermediary, options);
    }
}