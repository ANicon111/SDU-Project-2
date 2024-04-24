using Avalonia.Controls;
namespace HeatManagement.ViewModels;
using System;
using System.IO;
using System.Text.Json;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using HeatManagement.Views;
using ReactiveUI;

class ViewModelBase : ReactiveObject;

class MainWindowViewModel : ViewModelBase
{
    private UserControl currentPage = new Greeter();
    public UserControl CurrentPage { get => currentPage; set => this.RaiseAndSetIfChanged(ref currentPage, value); }

    private string error = "";
    public string Error { get => error; set => this.RaiseAndSetIfChanged(ref error, value); }


    public void GoToViewer()
    {
        CurrentPage = new ViewerGreeter();
    }

    public void GoToEditor()
    {
        CurrentPage = new EditorGreeter();
    }
    public string? ReadFile()
    {
        try
        {
            var topLevel = App.TopLevel;
            var files = topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
            {
                Title = "Open Text File",
                AllowMultiple = false
            }).GetAwaiter().GetResult();

            if (files.Count >= 1)
            {
                using var stream = files[0].OpenReadAsync().GetAwaiter().GetResult();
                using var streamReader = new StreamReader(stream);
                return streamReader.ReadToEnd();
            }
        }
        catch { }
        return null;
    }

    AssetManager assetManager = new();
    SourceDataManager sourceDataManager = new();
    ResultDataManager resultDataManager = new();

    public void OpenAssetFile()
    {
        string? fileContent = ReadFile();

        if (fileContent == null)
        {
            Error = "No file selected";
            return;
        }

        try
        {
            assetManager.JsonImport(fileContent!);
        }
        catch (FileNotFoundException)
        {
            assetManager = null;
            Error = "The Json file could not be found";
        }
        catch (JsonException)
        {
            assetManager = null;
            Error = "The Selected file is not a valid Json file";
        }
        catch (Exception ex)
        {
            assetManager = null;
            Error = $"An error has occurred {ex.Message}";
        }
    }

    public void OpenSourceFile()
    {
        string? fileContent = ReadFile();

        if (fileContent == null)
        {
            Error = "No file selected";
            return;
        }

        try
        {
            sourceDataManager.JsonImport(fileContent!);
        }
        catch (FileNotFoundException)
        {
            assetManager = null;
            Error = "The Json file could not be found";
        }
        catch (JsonException)
        {
            assetManager = null;
            Error = "The Selected file is not a valid Json file";
        }
        catch (Exception ex)
        {
            assetManager = null;
            Error = $"An error has occurred {ex.Message}";
        }
    }

}