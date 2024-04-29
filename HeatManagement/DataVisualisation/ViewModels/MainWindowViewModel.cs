using Avalonia.Controls;
namespace HeatManagement.ViewModels;
using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using HeatManagement.Views;
using ReactiveUI;

class ViewModelBase : ReactiveObject;

class MainWindowViewModel : ViewModelBase
{
    private UserControl currentPage = new Greeter();
    public UserControl CurrentPage { get => currentPage; set => this.RaiseAndSetIfChanged(ref currentPage, value); }


    private string? assetsError = null;
    public string? AssetsError { get => assetsError; set => this.RaiseAndSetIfChanged(ref assetsError, value); }
    private string? sourceError = null;
    public string? SourceError { get => sourceError; set => this.RaiseAndSetIfChanged(ref sourceError, value); }
    private string? resultError = null;
    public string? ResultError { get => resultError; set => this.RaiseAndSetIfChanged(ref resultError, value); }

    private string? assetsSuccess = null;
    public string? AssetsSuccess { get => assetsSuccess; set => this.RaiseAndSetIfChanged(ref assetsSuccess, value); }
    private string? sourceSuccess = null;
    public string? SourceSuccess { get => sourceSuccess; set => this.RaiseAndSetIfChanged(ref sourceSuccess, value); }
    private string? resultSuccess = null;
    public string? ResultSuccess { get => resultSuccess; set => this.RaiseAndSetIfChanged(ref resultSuccess, value); }

    private bool canOpenEditor = false;
    public bool CanOpenEditor { get => canOpenEditor; set => this.RaiseAndSetIfChanged(ref canOpenEditor, value); }
    private bool canOpenViewer = false;
    public bool CanOpenViewer { get => canOpenViewer; set => this.RaiseAndSetIfChanged(ref canOpenViewer, value); }

    public void GoToViewerGreeter()
    {
        CurrentPage = new ViewerGreeter();
    }

    public void GoToEditorGreeter()
    {
        CurrentPage = new EditorGreeter();
    }
    public void GoToViewer()
    {
        CurrentPage = new Viewer();
        new Optimizer(assetManager, sourceDataManager, resultDataManager).Optimize();
        _ = new ViewerViewModel(resultDataManager);
    }

    public void GoToEditor()
    {
        GoToAssetsEditor();
    }

    public void GoToAssetsEditor()
    {
        CurrentPage = new AssetEditor() { DataContext = new AssetEditorViewModel(assetManager) };
    }
    public static async Task<string?> ReadFile(string title)
    {
        var files = await App.TopLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = title,
            AllowMultiple = false
        });

        if (files.Count >= 1)
        {
            using var stream = await files[0].OpenReadAsync();
            using var streamReader = new StreamReader(stream);
            return streamReader.ReadToEnd();
        }
        return null;
    }

    AssetManager assetManager = new();
    SourceDataManager sourceDataManager = new();
    ResultDataManager resultDataManager = new();

    void UpdateOpenButtonState()
    {
        CanOpenViewer = resultDataManager.Loaded || assetManager.Loaded && sourceDataManager.Loaded;
        CanOpenEditor = assetManager.Loaded || sourceDataManager.Loaded;
    }

    public async void OpenAssetsFile()
    {
        AssetsError = null;
        AssetsSuccess = null;

        try
        {
            string? fileContent = await ReadFile("Open Asset File");

            if (fileContent == null)
            {
                AssetsError = "No file selected";
                return;
            }

            assetManager.JsonImport(fileContent!);
            AssetsSuccess = "File opened successfully";
        }
        catch (FileNotFoundException)
        {
            assetManager = new();
            AssetsError = "The Json file could not be found";
        }
        catch (JsonException)
        {
            assetManager = new();
            AssetsError = "The selected file is not a valid Json file";
        }
        catch (Exception ex)
        {
            assetManager = new();
            AssetsError = $"An error has occurred {ex.Message}";
        }

        UpdateOpenButtonState();
    }

    public async void OpenSourceFile()
    {
        SourceError = null;
        SourceSuccess = null;

        try
        {
            string? fileContent = await ReadFile("Open Source Data File");

            if (fileContent == null)
            {
                SourceError = "No file selected";
                return;
            }

            sourceDataManager.JsonImport(fileContent!);
            SourceSuccess = "File opened successfully";
        }
        catch (FileNotFoundException)
        {
            sourceDataManager = new();
            SourceError = "The Json file could not be found";
        }
        catch (JsonException)
        {
            sourceDataManager = new();
            SourceError = "The selected file is not a valid Json file";
        }
        catch (Exception ex)
        {
            sourceDataManager = new();
            SourceError = $"An error has occurred {ex.Message}";
        }

        UpdateOpenButtonState();
    }

    public async void OpenResultFile()
    {
        ResultError = null;
        ResultSuccess = null;

        try
        {
            string? fileContent = await ReadFile("Open Result Data File");

            if (fileContent == null)
            {
                ResultError = "No file selected";
                return;
            }

            resultDataManager.JsonImport(fileContent!);
            ResultSuccess = "File opened successfully";
        }
        catch (FileNotFoundException)
        {
            resultDataManager = new();
            ResultError = "The Json file could not be found";
        }
        catch (JsonException)
        {
            resultDataManager = new();
            ResultError = "The selected file is not a valid Json file";
        }
        catch (Exception ex)
        {
            resultDataManager = new();
            ResultError = $"An error has occurred {ex.Message}";
        }

        UpdateOpenButtonState();
    }
}