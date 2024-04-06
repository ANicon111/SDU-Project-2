using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
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
    100 //minimum
    + 350 * (1 - Math.Sin(Math.PI * time.DayOfYear / 366)) //season modifier
    + 550 * (1 - Math.Sin(Math.PI * time.Hour / 24)) //time of day modifier
    + 650 * rng.NextDouble(); //random value
    //I'm sure this is 100% accurate and not completely made up

    public static List<SourceData> GetSourceDataFromAPI(DateTime startTime, DateTime endTime)
    {
        List<SourceData> sourceData = [];
        using (var client = new HttpClient())
        {
            client.BaseAddress = new Uri("https://api.energidataservice.dk");
            client.DefaultRequestHeaders.Add("User-Agent", "Anything");
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var request = client.GetAsync(FormattableString.Invariant($"dataset/Elspotprices?offset=0&start={startTime:yyyy'-'MM'-'dd'T'HH':'mm}&end={endTime:yyyy'-'MM'-'dd'T'HH':'mm}&filter=%7B%22PriceArea%22:[%22DK1%22]%7D")).Result;
            request.EnsureSuccessStatusCode();

            APIData resultData = request.Content.ReadFromJsonAsync<APIData>().Result ?? throw new("Couldn't connect to the api");

            foreach (Record record in resultData.records)
            {
                sourceData.Add(new(record.HourDK, record.HourDK.AddHours(1), GenerateHeatDemand(record.HourDK), record.SpotPriceDKK));
            }
        }
        return sourceData;
    }
}