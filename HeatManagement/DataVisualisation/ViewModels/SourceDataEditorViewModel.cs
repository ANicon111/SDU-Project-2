using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using Avalonia.Platform.Storage;
using DynamicData;
using HeatManagement.Views;
using ReactiveUI;

namespace HeatManagement.ViewModels;

class SourceDataEditorViewModel : ViewModelBase
{
    private SourceDataManager sourceDataManager;
    private string newStartTime = "";
    public string NewStartTime { get => newStartTime; set => this.RaiseAndSetIfChanged(ref newStartTime, value); }
    private string newEndTime = "";
    public string NewEndTime { get => newEndTime; set => this.RaiseAndSetIfChanged(ref newEndTime, value); }
    private string newElectricityPrice = "";
    public string NewElectricityPrice { get => newElectricityPrice; set => this.RaiseAndSetIfChanged(ref newElectricityPrice, value); }
    private string newHeatDemand = "";
    public string NewHeatDemand { get => newHeatDemand; set => this.RaiseAndSetIfChanged(ref newHeatDemand, value); }
    private bool newSourceOpen;
    public bool NewSourceOpen { get => newSourceOpen; set => this.RaiseAndSetIfChanged(ref newSourceOpen, value); }
    private string newSourceError = "";
    public string NewSourceError { get => newSourceError; set => this.RaiseAndSetIfChanged(ref newSourceError, value); }
    private string importStartTime = "";
    public string ImportStartTime { get => importStartTime; set => this.RaiseAndSetIfChanged(ref importStartTime, value); }
    private string importEndTime = "";
    public string ImportEndTime { get => importEndTime; set => this.RaiseAndSetIfChanged(ref importEndTime, value); }
    private string importSourceError = "";
    public string ImportSourceError { get => importSourceError; set => this.RaiseAndSetIfChanged(ref importSourceError, value); }
    private bool importOpen;
    public bool ImportOpen { get => importOpen; set => this.RaiseAndSetIfChanged(ref importOpen, value); }

    public ObservableCollection<SourceDataEditorButtonViewModel> SourceDataButtonValues { get; } = [];
    private void DeleteButton(SourceDataEditorButtonViewModel button)
    {
        SourceDataButtonValues.Remove(button);
        sourceDataManager.DataRemove(button.StartTime);
    }
    public void OpenSourceDataAdder()
    {
        NewStartTime = "";
        NewEndTime = "";
        NewElectricityPrice = "";
        NewHeatDemand = "";
        NewSourceError = "";
        NewSourceOpen = true;
    }
    public void CloseSourceDataAdder()
    {
        NewSourceOpen = false;
    }
    public void AddSourceData()
    {
        if (!DateTime.TryParse(NewStartTime, out DateTime startTime))
        {
            NewSourceError = "Couldn't parse start time";
        };

        if (!DateTime.TryParse(NewEndTime, out DateTime endTime))
        {
            NewSourceError = "Couldn't parse end time";
        };

        if (!double.TryParse(NewElectricityPrice, CultureInfo.InvariantCulture, out double electricityPrice))
        {
            NewSourceError = "Couldn't parse electricity";
            return;
        }

        if (!double.TryParse(NewHeatDemand, CultureInfo.InvariantCulture, out double heatDemand))
        {
            NewSourceError = "Couldn't parse heat demand";
            return;
        }

        sourceDataManager.DataAdd(startTime, endTime, electricityPrice, heatDemand);
        SourceDataButtonValues.Add(new(startTime, endTime, DeleteButton));
        CloseSourceDataAdder();
    }
    public void OpenImport()
    {
        ImportStartTime = "";
        ImportEndTime = "";
        ImportSourceError = "";
        ImportOpen = true;
    }
    public void CloseImport()
    {
        ImportOpen = false;
    }
    public void ImportData()
    {
        if (!DateTime.TryParse(ImportStartTime, out DateTime startTime))
        {
            ImportSourceError = "Couldn't parse start time";
            return;
        };
        if (!DateTime.TryParse(ImportEndTime, out DateTime endTime))
        {
            ImportSourceError = "Couldn't parse end time";
            return;
        };

        SourceDataManager tempSourceDataManager;

        try
        {
            tempSourceDataManager = Utils.GetSourceDataFromAPI(startTime, endTime).GetAwaiter().GetResult();
        }
        catch
        {
            ImportSourceError = "Couldn't get the data from the api";
            return;
        }
        sourceDataManager.SourceData.Clear();
        SourceDataButtonValues.Clear();
        sourceDataManager = tempSourceDataManager;

        SourceDataButtonValues.AddRange(sourceDataManager.SourceData.Select((val) => new SourceDataEditorButtonViewModel(val.StartTime, val.EndTime, DeleteButton)));
        CloseImport();
    }

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
                if (file.Name.Split('.').Last() == "json")
                {
                    await streamWriter.WriteAsync(sourceDataManager.JsonExport());
                }
                else
                {
                    await streamWriter.WriteAsync(sourceDataManager.CSVExport());
                }
            }
            catch { }
        }
    }

    public SourceDataEditorViewModel(SourceDataManager sourceDataManager)
    {
        this.sourceDataManager = sourceDataManager;
        foreach (Source source in this.sourceDataManager.SourceData)
        {
            SourceDataButtonValues.Add(new(source.StartTime, source.EndTime, DeleteButton));
        }
    }
}

public class SourceDataEditorButtonViewModel : ViewModelBase
{
    public DateTime StartTime { get; }
    public DateTime EndTime { get; }
    private readonly Action<SourceDataEditorButtonViewModel> DeleteButton;
    public void Delete() => DeleteButton(this);

    public SourceDataEditorButtonViewModel(DateTime startTime, DateTime endTime, Action<SourceDataEditorButtonViewModel> deleteButton)
    {
        StartTime = startTime;
        EndTime = endTime;
        DeleteButton = deleteButton;
    }
}
