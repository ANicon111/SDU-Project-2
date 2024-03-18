namespace HeatManagement;

using System;
using System.Collections.Generic;
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

    public SortedDictionary<Tuple<DateTime, DateTime>, Dictionary<string, ResultData>> ResultData;

    public ResultDataManager(string json = "{}")
    {
        ResultData = [];
        foreach (var element in JsonSerializer.Deserialize<SortedDictionary<string, Dictionary<string, ResultData>>>(json)!)
        {
            ResultData.Add(JsonSerializer.Deserialize<Tuple<DateTime, DateTime>>(element.Key)!, element.Value);
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
        SortedDictionary<string, Dictionary<string, ResultData>> jsonIntermediary = [];
        foreach (var item in ResultData)
        {
            jsonIntermediary.Add(JsonSerializer.Serialize(item.Key), item.Value);
        }
        return JsonSerializer.Serialize(jsonIntermediary);
    }
}