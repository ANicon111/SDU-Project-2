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

    private string? dataError = null;
    public string? DataError { get => dataError; set => this.RaiseAndSetIfChanged(ref dataError, value); }
    private string? assetsError = null;
    public string? AssetsError { get => assetsError; set => this.RaiseAndSetIfChanged(ref assetsError, value); }

    private string? dataFileName = null;
    public string? DataFileName { get => dataFileName; set => this.RaiseAndSetIfChanged(ref dataFileName, value); }
    private string? assetsFileName = null;
    public string? AssetsFileName { get => assetsFileName; set => this.RaiseAndSetIfChanged(ref assetsFileName, value); }

    readonly FilePickerFileType JsonFile = new("Json Files")
    {
        Patterns = ["*.json"],
        AppleUniformTypeIdentifiers = ["public.json"],
        MimeTypes = ["application/json", "text/json"]
    };

    readonly FilePickerFileType CSVFile = new("CSV Files")
    {
        Patterns = ["*.csv"],
        AppleUniformTypeIdentifiers = ["public.csv"],
        MimeTypes = ["application/csv", "text/csv"]
    };


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

    AssetManager? assetManager = new();
    SourceDataManager? sourceDataManager = new();
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

    public async void LoadDataFile()
    {
        // Start async operation to open the dialog.
        System.Collections.Generic.IReadOnlyList<IStorageFile> files = await App.TopLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Open a source or result data file",
            AllowMultiple = false,
            FileTypeFilter = [CSVFile, JsonFile]
        });

        if (files.Count >= 1)
        {
            sourceDataManager = new();
            resultDataManager = new();
            DataFileName = null;
            DataError = null;
            string? text = null;
            try
            {
                // Open reading stream from the first file.
                await using Stream stream = await files[0].OpenReadAsync();
                using StreamReader streamReader = new(stream);
                // Reads all the content of file as a text.
                text = await streamReader.ReadToEndAsync();
            }
            catch
            {
                DataError = "Couldn't read file";
            }
        }


    }
}