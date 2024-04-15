using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
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

public class SourceDataManager
{
    public List<Source> SourceData = [];
    public void JsonImport(string json)
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

    public string JsonExport() => JsonSerializer.Serialize(SourceData);

    public void DataAdd(DateTime startTime, DateTime endTime, double heatDemand, double electricityPrice)
    {
        SourceData.Add(new Source(startTime, endTime, heatDemand, electricityPrice));
    }
    public void DataRemove(DateTime startTime)
    {
        SourceData.RemoveAll(source => source.StartTime == startTime);
    }
    public string CSVExport()
    {
        int rowCount = SourceData.Count;

        string[] rows = new string[rowCount + 1];
        rows[0] = "StartTime, EndTime, HeatDemand, ElectricityPrice";

        for (int i = 0; i < rowCount; ++i)
        {
            Source source = SourceData[i];
            rows[i + 1] = FormattableString.Invariant($"{source.StartTime}, {source.EndTime}, {source.HeatDemand}, {source.ElectricityPrice}");
        }
        return string.Join('\n', rows);
    }
    public void CSVImport(string stringCSV)
    {
        string[] rows = stringCSV.Split("\n");
        if (rows[0] != "StartTime, EndTime, HeatDemand, ElectricityPrice")
        {
            throw new Exception("Invalid CSV file");
        }

        for (int i = 1; i < rows.Length; ++i)
        {
            string[] rowElements = rows[i].Split(",");

            DateTime startTime = DateTime.Parse(rowElements[0]);
            DateTime endTime = DateTime.Parse(rowElements[1]);
            double heatDemand = double.Parse(rowElements[2]);
            double electricityPrice = double.Parse(rowElements[3]);

            SourceData.Add(new Source(startTime, endTime, heatDemand, electricityPrice));
        }

    }
}
