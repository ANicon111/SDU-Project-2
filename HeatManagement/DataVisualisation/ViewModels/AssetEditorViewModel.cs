using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using Avalonia.Media.Imaging;
using Avalonia.Platform;

namespace HeatManagement.ViewModels;

class AssetEditorViewModel : ViewModelBase
{
    private readonly AssetManager assetManager;
    private string newAssetName = "";
    private string newAssetImagePath = "";
    private string newAssetCost = "";
    private string newAssetHeat = "";
    private string newAssetElectricity = "";
    private string newAssetCO2 = "";
    private string newAssetAdditionalResources = "";
    public string NewAssetName { get => newAssetName; set => newAssetName = value; }
    public string NewAssetImagePath { get => newAssetImagePath; set => newAssetImagePath = value; }
    public string NewAssetCost { get => newAssetCost; set => newAssetCost = value; }
    public string NewAssetHeat { get => newAssetHeat; set => newAssetHeat = value; }
    public string NewAssetElectricity { get => newAssetElectricity; set => newAssetElectricity = value; }
    public string NewAssetCO2 { get => newAssetCO2; set => newAssetCO2 = value; }
    public string NewAssetAdditionalResources { get => newAssetAdditionalResources; set => newAssetAdditionalResources = value; }

    private void DeleteButton(AssetEditorButtonViewModel button)
    {
        AssetButtonValues.Remove(button);
        assetManager.DataRemove(button.AssetName);
    }
    public void OpenAssetAdder()
    {
        NewAssetName = "";
        NewAssetCost = "";
        NewAssetHeat = "";
        NewAssetElectricity = "";
        NewAssetCO2 = "";
        NewAssetAdditionalResources = "";
    }

    public ObservableCollection<AssetEditorButtonViewModel> AssetButtonValues { get; } = [];
    public AssetEditorViewModel(AssetManager assetManager)
    {
        Console.WriteLine(assetManager.JsonExport());
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