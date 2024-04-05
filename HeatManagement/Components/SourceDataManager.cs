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
        foreach (KeyValuePair<Tuple<DateTime, DateTime>, SourceData> dataElement in data!)
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
            //CSV comma splitter
            List<string> columns = [];
            int l = 0, r = 0;
            bool inQuote = false;
            while (r < rows[i].Length)
            {
                if (rows[i][r] == '"') inQuote = !inQuote;
                if (!inQuote && rows[i][r] == ',')
                {
                    columns.Add(rows[i][l..r]);
                    l = r + 1;
                }
                r++;
            }
            columns.Add(rows[i][l..r]);

            //remove extra quotes from columns containing commas
            for (int j = 0; j < columns.Count; j++)
            {
                try
                {
                    if (columns[j].Contains(',')) columns[j] = columns[j].Replace("\"\"", "\"")[1..^1];
                }
                catch
                {
                    throw new($"CSV: Invalid comma escape at row {i}, column {j + 1}");
                }
            }

            if (columns.Count != 4) throw new($"CSV: Invalid column count at row {i}");

            string temp = columns[0].Replace("\"", "");
            if (!DateTime.TryParse(temp, CultureInfo.InvariantCulture, out DateTime startTime))
                if (!DateTime.TryParse(temp, out startTime))
                    throw new($"CSV: Invalid StartTime at row {i}");

            temp = columns[1].Replace("\"", "");
            if (!DateTime.TryParse(temp, CultureInfo.InvariantCulture, out DateTime endTime))
                if (!DateTime.TryParse(temp, out endTime))
                    throw new($"CSV: Invalid EndTime at row {i}");

            temp = columns[2].Replace("\"", "").Replace(',', '.');
            if (!double.TryParse(temp, CultureInfo.InvariantCulture, out double heatDemand))
                throw new($"CSV: Invalid HeatDemand at row {i}");

            temp = columns[3].Replace("\"", "").Replace(',', '.');
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
        List<string[]> table = [["StartTime", "EndTime", "HeatDemand", "ElectricityPrice"]];

        //values
        foreach (KeyValuePair<Tuple<DateTime, DateTime>, SourceData> data in Data)
        {
            table.Add([$"{data.Value.StartTime:O}", $"{data.Value.EndTime:O}", $"{data.Value.HeatDemand}", $"{data.Value.ElectricityPrice}"]);
        }

        //escape commas and quotes
        string[] rows = new string[table.Count];
        for (int i = 0; i < table.Count; i++)
        {
            for (int j = 0; j < table[i].Length; j++)
            {
                if (table[i][j].Contains(',')) table[i][j] = $"\"{table[i][j].Replace("\"", "\"\"")}\"";
            }
            rows[i] = string.Join(',', table[i]);
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