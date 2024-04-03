using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Text.Json;
using Avalonia.Platform.Storage;
using ReactiveUI;

namespace HeatManagement.GUI;

class SourceDataEditorViewModel : ViewModelBase
{
    private readonly SourceDataManager SourceData;
    public ObservableCollection<SourceDataEditorElementViewModel> SourceButtonValues { get; }
    public double BaseSize { get; }
    public double TitleSize { get; }

    //new sourceData variables
    private bool newSourceOpen = false;
    public bool NewSourceOpen { get => newSourceOpen; set => this.RaiseAndSetIfChanged(ref newSourceOpen, value); }
    private string? newSourceError = null;
    public string? NewSourceError { get => newSourceError; set => this.RaiseAndSetIfChanged(ref newSourceError, value); }

    private string newSourceStartTime = "";
    public string NewSourceStartTime { get => newSourceStartTime; set => this.RaiseAndSetIfChanged(ref newSourceStartTime, value); }
    private string newSourceEndTime = "";
    public string NewSourceEndTime { get => newSourceEndTime; set => this.RaiseAndSetIfChanged(ref newSourceEndTime, value); }
    private string newSourceHeatUsage = "";
    public string NewSourceHeatUsage { get => newSourceHeatUsage; set => this.RaiseAndSetIfChanged(ref newSourceHeatUsage, value); }
    private string newSourceElectricityCost = "";
    public string NewSourceElectricityCost { get => newSourceElectricityCost; set => this.RaiseAndSetIfChanged(ref newSourceElectricityCost, value); }


    public SourceDataEditorViewModel(double baseSize, SourceDataManager sourceData)
    {
        SourceData = sourceData;
        BaseSize = baseSize;
        TitleSize = BaseSize * 2;
        SourceButtonValues = [];
        foreach (var source in SourceData.Data)
        {
            SourceButtonValues.Add(new(BaseSize, source.Value.StartTime, source.Value.EndTime, RemoveSourceData));
        }
    }

    public void OpenSourceDataAdder()
    {
        NewSourceStartTime = "";
        NewSourceEndTime = "";
        NewSourceHeatUsage = "";
        NewSourceElectricityCost = "";
        NewSourceError = null;
        NewSourceOpen = true;
    }
    private string? TryParseSource()
    {
        if (!DateTime.TryParseExact(NewSourceStartTime, "dd.MM.yyyy HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.AllowWhiteSpaces, out DateTime startTime))
            if (!DateTime.TryParseExact(NewSourceStartTime, "dd.MM.yyyy HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.AllowWhiteSpaces, out startTime))
                if (!DateTime.TryParse(NewSourceStartTime, out startTime))
                    if (!DateTime.TryParse(NewSourceStartTime, CultureInfo.InvariantCulture, out startTime)) return "Invalid start time";
        if (!DateTime.TryParseExact(NewSourceEndTime, "dd.MM.yyyy HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.AllowWhiteSpaces, out DateTime endTime))
            if (!DateTime.TryParseExact(NewSourceEndTime, "dd.MM.yyyy HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.AllowWhiteSpaces, out endTime))
                if (!DateTime.TryParse(NewSourceEndTime, out endTime))
                    if (!DateTime.TryParse(NewSourceEndTime, CultureInfo.InvariantCulture, out endTime)) return "Invalid end time";
        if (endTime <= startTime) return "End time cannot be before or equal to the start time";
        if (SourceData.Data.ContainsKey(Tuple.Create(startTime, endTime))) return "Start time and end time are already present";

        if (!double.TryParse(NewSourceHeatUsage, out double heatConsumption))
            if (!double.TryParse(NewSourceHeatUsage, CultureInfo.InvariantCulture, out heatConsumption)) return "Invalid heat usage";

        if (!double.TryParse(NewSourceElectricityCost, out double electricityCost))
            if (!double.TryParse(NewSourceElectricityCost, CultureInfo.InvariantCulture, out electricityCost)) return "Invalid electricity cost";

        SourceData.AddData(new(startTime, endTime, heatConsumption, electricityCost));
        int i = 0;
        while (i < SourceButtonValues.Count && startTime > SourceButtonValues[i].StartTime) i++;
        SourceButtonValues.Insert(i, new(BaseSize, startTime, endTime, RemoveSourceData));

        NewSourceOpen = false;
        return null;
    }

    public void AddSourceData()
    {
        NewSourceError = TryParseSource();
    }

    public void RemoveSourceData(DateTime startTime, DateTime endTime, SourceDataEditorElementViewModel element)
    {
        SourceData.RemoveData(startTime, endTime);
        SourceButtonValues.Remove(element);
    }

    private readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };
    public async void ExportSourceData()
    {
        // Start async operation to open the dialog.
        IStorageFile? file = await App.TopLevel.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            Title = "Export source data",
            SuggestedFileName = "sourceData.json"
        });

        if (file != null)
        {
            try
            {
                // Open writing stream from the file.
                await using Stream stream = await file.OpenWriteAsync();
                using StreamWriter streamWriter = new(stream);
                // Writes all the content of file as a text.
                await streamWriter.WriteAsync(SourceData.ToJson(JsonOptions));
            }
            catch { }
        }
    }
}

class SourceDataEditorElementViewModel(double baseSize, DateTime startTime, DateTime endTime, Action<DateTime, DateTime, SourceDataEditorElementViewModel> removeSourceData) : ViewModelBase
{
    public double BaseSize { get; } = baseSize;
    public readonly DateTime StartTime = startTime;
    public readonly DateTime EndTime = endTime;
    public string TimeRange { get; } = $"{startTime:dd'.'MM'.'yyyy' 'HH':'mm':'ss} - {endTime:dd'.'MM'.'yyyy' 'HH':'mm':'ss}";
    private readonly Action<DateTime, DateTime, SourceDataEditorElementViewModel> removeSourceData = removeSourceData;
    public void RemoveSourceData() => removeSourceData(StartTime, EndTime, this);
}