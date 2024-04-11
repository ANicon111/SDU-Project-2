using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace HeatManagement;

public static class Utils
{
    public static string TableToCSV(List<string[]> table)
    {
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
        return string.Join(/*Environment.NewLine*/ '\n', rows); //no mercy for windows users, \r\n is cringe
    }

    private class Record
    {
        public DateTime HourUTC { get; set; }
        public required DateTime HourDK { get; set; }
        public required double SpotPriceDKK { get; set; }
        public double SpotPriceEUR { get; set; }
    }

#pragma warning disable IDE1006 // Naming Styles

    private class APIData
    {
        public int total { get; set; }
        public string filters { get; set; } = "";
        public string dataset { get; set; } = "";
        public required List<Record> records { get; set; } = [];
    }

#pragma warning restore IDE1006 // Naming Styles

    private static readonly Random rng = new();
    private static double GenerateHeatDemand(DateTime time) =>
    1 //minimum
    + 5 * (1 - Math.Sin(Math.PI * time.DayOfYear / 366)) //season modifier
    + 1 * (1 - Math.Sin(Math.PI * time.Hour / 24)) //time of day modifier
    + 2 * rng.NextDouble(); //random value
    //I'm sure this is 100% accurate and not completely made up

    public static async Task<SourceDataManager> GetSourceDataFromAPI(DateTime startTime, DateTime endTime)
    {
        SourceDataManager sourceData = new();
        using (var client = new HttpClient())
        {
            client.BaseAddress = new Uri("https://api.energidataservice.dk");
            client.DefaultRequestHeaders.Add("User-Agent", "Anything");
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            HttpResponseMessage request = await client.GetAsync(FormattableString.Invariant($"dataset/Elspotprices?offset=0&start={startTime:yyyy'-'MM'-'dd'T'HH':'mm}&end={endTime:yyyy'-'MM'-'dd'T'HH':'mm}&filter=%7B%22PriceArea%22:[%22DK1%22]%7D"));
            request.EnsureSuccessStatusCode();

            APIData resultData = await request.Content.ReadFromJsonAsync<APIData>() ?? throw new("Couldn't connect to the api");

            foreach (Record record in resultData.records)
            {
                try
                {
                    sourceData.AddData(new(record.HourDK, record.HourDK.AddHours(1), GenerateHeatDemand(record.HourDK), record.SpotPriceDKK));
                }
                catch { }
            }
        }
        return sourceData;
    }
}