using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
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
    public bool Loaded { get; private set; }
    public void JsonImport(string json)
    {
        Loaded = false;
        SourceData = JsonSerializer.Deserialize<List<Source>>(json) ?? throw new("Null json result");
        Loaded = true;
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
        rows[0] = "StartTime,EndTime,HeatDemand,ElectricityPrice";

        for (int i = 0; i < rowCount; ++i)
        {
            Source source = SourceData[i];
            rows[i + 1] = FormattableString.Invariant($"{source.StartTime:s},{source.EndTime:s},{source.HeatDemand},{source.ElectricityPrice}");
        }
        return string.Join('\n', rows);
    }
    public void CSVImport(string stringCSV)
    {
        Loaded = false;
        string[] rows = stringCSV.Replace("\r\n", "\n").Split("\n");
        /* spell-checker: disable */
        if (rows[0].Replace(" ", "").ToLower() != "starttime,endtime,heatdemand,electricityprice")
        /* spell-checker: enable */
        {
            throw new Exception($"Invalid CSV header");
        }

        for (int i = 1; i < rows.Length; ++i)
        {
            if (!string.IsNullOrWhiteSpace(rows[i]))
            {
                string[] rowElements = rows[i].Split(",");

                DateTime startTime = DateTime.Parse(rowElements[0]);
                DateTime endTime = DateTime.Parse(rowElements[1]);
                double heatDemand = double.Parse(rowElements[2], CultureInfo.InvariantCulture);
                double electricityPrice = double.Parse(rowElements[3], CultureInfo.InvariantCulture);

                SourceData.Add(new Source(startTime, endTime, heatDemand, electricityPrice));
            }
        }
        Loaded = true;
    }
}
