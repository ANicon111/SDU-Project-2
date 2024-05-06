using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.Json;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Platform.Storage;
using HeatManagement.Views;
using ReactiveUI;

namespace HeatManagement.ViewModels;

class AssetEditorViewModel : ViewModelBase
{
    private readonly AssetManager assetManager;
    private string newAssetName = "";
    public string NewAssetName { get => newAssetName; set => this.RaiseAndSetIfChanged(ref newAssetName, value); }
    private string newImagePath = "";
    public string NewImagePath { get => newImagePath; set => this.RaiseAndSetIfChanged(ref newImagePath, value); }
    private string newAssetImagePath = "";
    public string NewAssetImagePath { get => newAssetImagePath; set => this.RaiseAndSetIfChanged(ref newAssetImagePath, value); }
    private string newAssetCost = "";
    public string NewAssetCost { get => newAssetCost; set => this.RaiseAndSetIfChanged(ref newAssetCost, value); }
    private string newAssetHeat = "";
    public string NewAssetHeat { get => newAssetHeat; set => this.RaiseAndSetIfChanged(ref newAssetHeat, value); }
    private string newAssetElectricity = "";
    public string NewAssetElectricity { get => newAssetElectricity; set => this.RaiseAndSetIfChanged(ref newAssetElectricity, value); }
    private string newAssetCO2 = "";
    public string NewAssetCO2 { get => newAssetCO2; set => this.RaiseAndSetIfChanged(ref newAssetCO2, value); }
    private string newAssetAdditionalResources = "";
    public string NewAssetAdditionalResources { get => newAssetAdditionalResources; set => this.RaiseAndSetIfChanged(ref newAssetAdditionalResources, value); }
    private bool newAssetOpen = false;
    public bool NewAssetOpen { get => newAssetOpen; set => this.RaiseAndSetIfChanged(ref newAssetOpen, value); }
    private string newAssetError = "";
    public string NewAssetError { get => newAssetError; set => this.RaiseAndSetIfChanged(ref newAssetError, value); }

    private void DeleteButton(AssetEditorButtonViewModel button)
    {
        AssetButtonValues.Remove(button);
        assetManager.DataRemove(button.AssetName);
    }
    public void OpenAssetAdder()
    {
        NewAssetName = "";
        NewImagePath = "";
        NewAssetCost = "";
        NewAssetHeat = "";
        NewAssetElectricity = "";
        NewAssetCO2 = "";
        NewAssetAdditionalResources = "";
        NewAssetError = "";
        NewAssetOpen = true;
    }

    public void CloseAssetAdder()
    {
        NewAssetOpen = false;
    }

    public void AddAsset()
    {
        string name = NewAssetName.Trim().ToUpper();
        string imagePath = NewImagePath;

        if (!double.TryParse(NewAssetCost, CultureInfo.InvariantCulture, out double cost))
        {
            NewAssetError = "Couldn't parse cost";
            return;
        }

        if (!double.TryParse(NewAssetHeat, CultureInfo.InvariantCulture, out double heat))
        {
            NewAssetError = "Couldn't parse heat";
            return;
        }

        if (!double.TryParse(NewAssetElectricity, CultureInfo.InvariantCulture, out double electricity))
        {
            NewAssetError = "Couldn't parse electricity";
            return;
        }

        if (!double.TryParse(NewAssetCO2, CultureInfo.InvariantCulture, out double co2))
        {
            NewAssetError = "Couldn't parse CO2";
            return;
        }

        Dictionary<string, double> additionalResources = [];

        //gas: 2.3, oil: 3.2
        try
        {
            if (!string.IsNullOrWhiteSpace(NewAssetAdditionalResources))
                foreach (string element in NewAssetAdditionalResources.Split(','))
                {
                    string[] elements = element.Split(':');
                    string resourceNameRaw = elements[0].Trim().ToLower();
                    string resourceName = char.ToUpper(resourceNameRaw[0]) + resourceNameRaw[1..];
                    if (!double.TryParse(elements[1], CultureInfo.InvariantCulture, out double resourceValue))
                    {
                        NewAssetError = "Couldn't parse additional resources";
                        return;
                    }
                    additionalResources.Add(resourceName, resourceValue);
                }
        }
        catch
        {
            NewAssetError = "Invalid additional resources";
            return;
        }

        assetManager.DataAdd(name, imagePath, heat, cost, electricity, co2, additionalResources);
        AssetButtonValues.Add(new(name, imagePath, DeleteButton));
        CloseAssetAdder();
    }

    private readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };
    public async void ExportAssets()
    {
        // Start async operation to open the dialog.
        IStorageFile? file = await App.TopLevel.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            Title = "Export assets",
            SuggestedFileName = "assets.json"
        });

        if (file != null)
        {
            try
            {
                // Open writing stream from the file.
                await using Stream stream = await file.OpenWriteAsync();
                using StreamWriter streamWriter = new(stream);
                await streamWriter.WriteAsync(assetManager.JsonExport());
            }
            catch { }
        }
    }

    public ObservableCollection<AssetEditorButtonViewModel> AssetButtonValues { get; } = [];
    public AssetEditorViewModel(AssetManager assetManager)
    {
        this.assetManager = assetManager;
        foreach (Asset asset in this.assetManager.Assets)
        {
            AssetButtonValues.Add(new(asset.Name, asset.ImagePath, DeleteButton));
        }
    }
}

class AssetEditorButtonViewModel : ViewModelBase
{
    public string AssetName { get; }
    public Bitmap Image { get; }

    private readonly Action<AssetEditorButtonViewModel> DeleteButton;
    public void Delete() => DeleteButton(this);

    public AssetEditorButtonViewModel(string assetName, string imagePath, Action<AssetEditorButtonViewModel> deleteButton)
    {
        DeleteButton = deleteButton;
        AssetName = assetName;
        Image = new(AssetLoader.Open(new Uri("avares://HeatManagement/DataVisualisation/Assets/Resources/unknown.png")));
        try
        {
            Image = new(imagePath);
        }
        catch { }
    }
}