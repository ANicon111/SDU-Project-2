namespace HeatManagement;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;


public struct ResultData(double heat, double cost, double electricity, double co2, Dictionary<string, double> additionalResources)
{
    private double heat = heat;
    private double cost = cost;
    private double electricity = electricity;
    private double co2 = co2;
    private Dictionary<string, double> additionalResources = additionalResources;

    public double Heat { readonly get => heat; set => heat = value; }
    public double Cost { readonly get => cost; set => cost = value; }
    public double Electricity { readonly get => electricity; set => electricity = value; }
    public double CO2 { readonly get => co2; set => co2 = value; }
    public Dictionary<string, double> AdditionalResources { readonly get => additionalResources; set => additionalResources = value; }
}

public class ResultDataManager
{

    public SortedDictionary<Tuple<DateTime, DateTime>, Dictionary<string, ResultData>> RawResultData;
    public SortedDictionary<Tuple<DateTime, DateTime>, ResultData> CompiledResultData;

    public ResultDataManager(string json = "{}")
    {
        RawResultData = [];
        foreach (var element in JsonSerializer.Deserialize<SortedDictionary<string, Dictionary<string, ResultData>>>(json)!)
        {
            RawResultData.Add(JsonSerializer.Deserialize<Tuple<DateTime, DateTime>>(element.Key)!, element.Value);
        }
        CompiledResultData = [];
        CompileData();
    }

    public void CompileData()
    {
        CompiledResultData = [];
        foreach (KeyValuePair<Tuple<DateTime, DateTime>, Dictionary<string, ResultData>> RawResult in RawResultData)
        {
            ResultData compiledResult = new(0, 0, 0, 0, []);
            foreach (KeyValuePair<string, ResultData> AssetResult in RawResult.Value)
            {
                compiledResult.Heat += AssetResult.Value.Heat;
                compiledResult.Cost += AssetResult.Value.Cost;
                compiledResult.Electricity += AssetResult.Value.Electricity;
                compiledResult.CO2 += AssetResult.Value.CO2;
                foreach (KeyValuePair<string, double> additionalResource in compiledResult.AdditionalResources)
                {
                    if (!compiledResult.AdditionalResources.ContainsKey(additionalResource.Key))
                        compiledResult.AdditionalResources[additionalResource.Key] = 0;
                    compiledResult.AdditionalResources[additionalResource.Key] += additionalResource.Value;
                }
            }
            CompiledResultData[Tuple.Create(RawResult.Key.Item1, RawResult.Key.Item2)] = compiledResult;
        }
    }

    public void AddData(DateTime startTime, DateTime endTime, string assetName, ResultData assetData)
    {
        if (!RawResultData.ContainsKey(Tuple.Create(startTime, endTime))) RawResultData[Tuple.Create(startTime, endTime)] = [];
        if (!RawResultData[Tuple.Create(startTime, endTime)].ContainsKey(assetName)) RawResultData[Tuple.Create(startTime, endTime)][assetName] = assetData;
    }

    public void RemoveDataByTime(DateTime startTime, DateTime endTime, string? assetName = null)
    {
        if (assetName != null)
        {
            RawResultData[Tuple.Create(startTime, endTime)].Remove(assetName);
            ResultData compiledResult = new(0, 0, 0, 0, []);
            foreach (KeyValuePair<string, ResultData> AssetResult in RawResultData[Tuple.Create(startTime, endTime)])
            {
                compiledResult.Heat += AssetResult.Value.Heat;
                compiledResult.Cost += AssetResult.Value.Cost;
                compiledResult.Electricity += AssetResult.Value.Electricity;
                compiledResult.CO2 += AssetResult.Value.CO2;
                foreach (KeyValuePair<string, double> additionalResource in compiledResult.AdditionalResources)
                {
                    if (!compiledResult.AdditionalResources.ContainsKey(additionalResource.Key))
                        compiledResult.AdditionalResources[additionalResource.Key] = 0;
                    compiledResult.AdditionalResources[additionalResource.Key] += additionalResource.Value;
                }
            }
            CompiledResultData[Tuple.Create(startTime, endTime)] = compiledResult;
        }
        else
        {
            RawResultData.Remove(Tuple.Create(startTime, endTime));
        }
    }

    public string ToJson()
    {
        SortedDictionary<string, Dictionary<string, ResultData>> jsonIntermediary = [];
        foreach (var item in RawResultData)
        {
            jsonIntermediary.Add(JsonSerializer.Serialize(item.Key), item.Value);
        }
        return JsonSerializer.Serialize(jsonIntermediary);
    }
}