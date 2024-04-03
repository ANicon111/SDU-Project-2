using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Text.Json;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Platform.Storage;
using ReactiveUI;

namespace HeatManagement.GUI;

class AssetsEditorViewModel : ViewModelBase
{
    private readonly AssetManager Assets;
    public ObservableCollection<AssetsEditorElementViewModel> AssetButtonValues { get; }
    public double BaseSize { get; }
    public double TitleSize { get; }

    //new asset variables
    private bool newAssetOpen = false;
    public bool NewAssetOpen { get => newAssetOpen; set => this.RaiseAndSetIfChanged(ref newAssetOpen, value); }
    private string? newAssetError = null;
    public string? NewAssetError { get => newAssetError; set => this.RaiseAndSetIfChanged(ref newAssetError, value); }

    private string newAssetName = "";
    public string NewAssetName { get => newAssetName; set => this.RaiseAndSetIfChanged(ref newAssetName, value); }
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


    public AssetsEditorViewModel(double baseSize, AssetManager assets)
    {
        Assets = assets;
        BaseSize = baseSize;
        TitleSize = BaseSize * 2;
        AssetButtonValues = [];
        foreach (var asset in Assets.Assets)
        {
            AssetButtonValues.Add(new(BaseSize, asset.Key, asset.Value.ImagePath, RemoveAsset));
        }
    }

    public void OpenAssetAdder()
    {
        NewAssetName = "";
        NewAssetCost = "";
        NewAssetHeat = "";
        NewAssetElectricity = "";
        NewAssetCO2 = "";
        NewAssetAdditionalResources = "";
        NewAssetError = null;
        NewAssetOpen = true;
    }

    private string? tryParseAsset()
    {
        string name = NewAssetName.Trim().ToUpper();
        if (Assets.Assets.ContainsKey(name)) return "Asset name already exists";
        if (!double.TryParse(NewAssetHeat, out double heat))
            if (!double.TryParse(NewAssetHeat, CultureInfo.InvariantCulture, out heat)) return "Invalid heat";
        if (!double.TryParse(NewAssetCost, out double cost))
            if (!double.TryParse(NewAssetCost, CultureInfo.InvariantCulture, out cost)) return "Invalid cost";
        if (!double.TryParse(NewAssetElectricity, out double electricity))
            if (!double.TryParse(NewAssetElectricity, CultureInfo.InvariantCulture, out electricity)) return "Invalid electricity production/consumption";
        if (!double.TryParse(NewAssetCO2, out double co2))
            if (!double.TryParse(NewAssetCO2, CultureInfo.InvariantCulture, out co2)) return "Invalid co2 emissions";
        Dictionary<string, double> additionalResources = [];
        try
        {
            if (!string.IsNullOrWhiteSpace(NewAssetAdditionalResources))
            {
                foreach (string resource in NewAssetAdditionalResources.Split(','))
                {
                    string[] resourceNameAndValue = resource.Split(':');
                    string resourceName = resourceNameAndValue[0].Trim().ToLower();
                    if (!double.TryParse(resourceNameAndValue[1], out double resourceValue))
                        if (!double.TryParse(resourceNameAndValue[1], CultureInfo.InvariantCulture, out resourceValue))
                            return $"Invalid additional resources";
                    additionalResources.Add(resourceName, resourceValue);
                }
            }
        }
        catch
        {
            return "Invalid additional resources";
        }
        Assets.AddAsset(name, new(NewAssetImagePath, heat, cost, electricity, co2, additionalResources));
        AssetButtonValues.Add(new(BaseSize, name, NewAssetImagePath, RemoveAsset));
        NewAssetOpen = false;
        return null;
    }

    public void AddAsset()
    {
        NewAssetError = tryParseAsset();
    }

    public void RemoveAsset(string assetName, AssetsEditorElementViewModel element)
    {
        Assets.RemoveAsset(assetName);
        AssetButtonValues.Remove(element);

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
                // Writes all the content of file as a text.
                await streamWriter.WriteAsync(Assets.ToJson(JsonOptions));
            }
            catch { }
        }
    }
}

class AssetsEditorElementViewModel : ViewModelBase
{
    public double BaseSize { get; }
    public double LargeImageSize { get; }
    public string AssetName { get; }
    public Bitmap Image { get; }

    private readonly Action<string, AssetsEditorElementViewModel> removeAsset;
    public void RemoveAsset() => removeAsset(AssetName, this);

    public AssetsEditorElementViewModel(double baseSize, string assetName, string imagePath, Action<string, AssetsEditorElementViewModel> removeAsset)
    {
        this.removeAsset = removeAsset;
        BaseSize = baseSize;
        LargeImageSize = baseSize * 5;
        AssetName = assetName;
        Image = new Bitmap(AssetLoader.Open(new Uri($"avares://HeatManagement/GUI/Assets/Resources/unknown.png")));
        try { Image = new(File.OpenRead(imagePath)); } catch { }
    }
}