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

    public ObservableCollection<AssetEditorButtonViewModel> AssetButtonValues { get; } = [];
    public AssetEditorViewModel(AssetManager assetManager)
    {
        Console.WriteLine(assetManager.JsonExport());
        this.assetManager = assetManager;
        foreach (Asset asset in this.assetManager.Assets)
        {
            AssetButtonValues.Add(new(asset.Name, asset.ImagePath));
        }
    }
}

class AssetEditorButtonViewModel : ViewModelBase
{
    public string AssetName { get; }
    public Bitmap Image { get; }

    public AssetEditorButtonViewModel(string assetName, string imagePath)
    {
        AssetName = assetName;
        Image = new(AssetLoader.Open(new Uri("avares://HeatManagement/DataVisualisation/Assets/Resources/unknown.png")));
        try
        {
            Image = new(imagePath);
        }
        catch { }
    }
}