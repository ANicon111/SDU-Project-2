using System;
using System.Collections.Generic;
using System.Text.Json;

namespace HeatManagement;

// Result data should be separated for each production unit and should contain important
// information such as produced amount of heat, produced / consumed electricity,
// production costs, consumption of primary energy and produced amount of CO2 
public struct Result(string unit, double producedHeat, double consumedElectricity, double cost, double consumptionOfEnergy, double producedCO2, Dictionary<string, double> additionalResources)
{
    private string unit = unit;
    private double producedHeat = producedHeat;
    private double consumedElectricity = consumedElectricity;
    private double cost = cost;
    private double consumptionOfEnergy = consumptionOfEnergy;
    private double producedCO2 = producedCO2;
    private Dictionary<string, double> additionalResources = additionalResources;


    public string Unit { readonly get => unit; set => unit = value; }
    public double ProducedHeat { readonly get => producedHeat; set => producedHeat = value; }
    public double ConsumedElectricity { readonly get => consumedElectricity; set => consumedElectricity = value; }
    public double Cost { readonly get => cost; set => cost = value; }
    public double ConsumptionOfEnergy { readonly get => consumptionOfEnergy; set => consumptionOfEnergy = value; }
    public double ProducedCO2 { readonly get => producedCO2; set => producedCO2 = value; }
    public Dictionary<string, double> AdditionalResources { readonly get => additionalResources; set => additionalResources = value; }
}

public struct UnitResults(DateTime startTime, DateTime endTime, List<Result> results)
{
    private DateTime startTime = startTime;
    private DateTime endTime = endTime;
    private List<Result> results = results;

    public DateTime StartTime { readonly get => startTime; set => startTime = value; }
    public DateTime EndTime { readonly get => endTime; set => endTime = value; }
    public List<Result> Results { readonly get => results; set => results = value; }
}

class ResultDataManager
{
    public static List<UnitResults> TimeResults = [];

    public void JsonImport(string json)
    {
        try
        {
            TimeResults = JsonSerializer.Deserialize<List<UnitResults>>(json)!;
        }
        catch
        {
            throw new Exception("Error: Failed to parse Json");
        }
    }
    public string JsonExport() => JsonSerializer.Serialize(TimeResults);

    public void DataAddition(UnitResults results)
    {
        TimeResults.Add(results);

    }
    public void DataRemoval(DateTime startTime)
    {
        TimeResults.RemoveAll(result => result.StartTime == startTime);
    }

}