namespace HeatManagement;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
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
    public SortedDictionary<Tuple<DateTime, DateTime>, SourceData> Data { get => data; set => data = value; }

    private SourceDataManager(SortedDictionary<Tuple<DateTime, DateTime>, SourceData> data)
    {
        this.data = data;
    }

    public SourceDataManager()
    {
        data = [];
    }

    public static SourceDataManager FromJson(string json)
    {
        SortedDictionary<Tuple<DateTime, DateTime>, SourceData> data = [];
        try
        {
            List<SourceData> jsonIntermediary = JsonSerializer.Deserialize<List<SourceData>>(json) ?? throw new("Null value");
            foreach (SourceData dataElement in jsonIntermediary)
            {
                data.Add(Tuple.Create(dataElement.StartTime, dataElement.EndTime), dataElement);
            }
        }
        catch
        {
            throw new("Invalid Data");
        }
        return new(data);
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

    public static SourceDataManager FromCSV(string csv)
    {
        string[] rows = csv.Split(["\n\r", "\r\n", "\r", "\n"], StringSplitOptions.None);

        if (rows.Length == 0) throw new("Invalid Data");

        //check header
        if (rows[0] != "StartTime,EndTime,HeatDemand,ElectricityPrice")
            throw new("Invalid Data");

        SortedDictionary<Tuple<DateTime, DateTime>, SourceData> data = [];

        for (int i = 1; i < rows.Length; i++)
        {
            string[] columns = rows[i].Split(',');
            if (columns.Length != 4) throw new($"CSV: Invalid column count at row {i}");

            string temp = columns[0].Replace("\"", "");
            if (!DateTime.TryParse(temp, CultureInfo.InvariantCulture, out DateTime startTime))
                throw new($"CSV: Invalid StartTime at row {i}");

            temp = columns[1].Replace("\"", "");
            if (!DateTime.TryParse(temp, CultureInfo.InvariantCulture, out DateTime endTime))
                throw new($"CSV: Invalid EndTime at row {i}");

            temp = columns[2].Replace("\"", "");
            if (!double.TryParse(temp, CultureInfo.InvariantCulture, out double heatDemand))
                throw new($"CSV: Invalid HeatDemand at row {i}");

            temp = columns[3].Replace("\"", "");
            if (!double.TryParse(temp, CultureInfo.InvariantCulture, out double electricityPrice))
                throw new($"CSV: Invalid ElectricityPrice at row {i}");

            if (!data.TryAdd(Tuple.Create(startTime, endTime), new(startTime, endTime, heatDemand, electricityPrice)))
                throw new($"CSV: Duplicate source data times at row {i}");

        }
        return new(data);
    }

    public string ToCSV()
    {
        //standard header
        List<string> rows = ["StartTime,EndTime,HeatDemand,ElectricityPrice"];

        //values
        foreach (KeyValuePair<Tuple<DateTime, DateTime>, SourceData> data in Data)
        {
            rows.Add(FormattableString.Invariant($"{data.Value.StartTime:O},{data.Value.EndTime:O},{data.Value.HeatDemand},{data.Value.ElectricityPrice}"));
        }

        return string.Join('\n', rows);
    }

    public static SourceDataManager FromAnySupportedFormat(string text)
    {
        SourceDataManager? sourceDataManager = null;

        try
        {
            sourceDataManager = FromJson(text);
        }
        catch (Exception e)
        {
            if (e.Message != "Invalid Data")
                throw;
        }

        try
        {
            sourceDataManager = FromCSV(text);
        }
        catch (Exception e)
        {
            if (e.Message != "Invalid Data")
                throw;
        }

        if (sourceDataManager == null) throw new("Invalid Data");
        return sourceDataManager;
    }

    public void AddData(SourceData sourceData)
    {
        data?.Add(Tuple.Create(sourceData.StartTime, sourceData.EndTime), sourceData);
    }
    public void RemoveData(DateTime startTime, DateTime endTime)
    {
        data?.Remove(Tuple.Create(startTime, endTime));
    }
}