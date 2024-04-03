using System;
using System.IO;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using ReactiveUI;
namespace HeatManagement.GUI;

class GreeterViewModel : ViewModelBase
{
    public Arguments Arguments { get; set; }

    private UserControl currentPage = new GreeterView();
    public UserControl CurrentPage { get => currentPage; set => this.RaiseAndSetIfChanged(ref currentPage, value); }

    //Managers
    SourceDataManager? SourceData = null;
    ResultDataManager? ResultData = null;
    AssetManager? Assets = null;

    //Design
    public double BaseSize { get; } = MainWindow.FontSize * 2;
    public double H1Size { get; }
    public double LargeImageSize { get; }

    //ViewerGreeter
    private string? dataError = null;
    public string? DataError { get => dataError; set => this.RaiseAndSetIfChanged(ref dataError, value); }
    private string? assetsError = null;
    public string? AssetsError { get => assetsError; set => this.RaiseAndSetIfChanged(ref assetsError, value); }

    private string? dataFileName = null;
    public string? DataFileName { get => dataFileName; set => this.RaiseAndSetIfChanged(ref dataFileName, value); }
    private string? assetsFileName = null;
    public string? AssetsFileName { get => assetsFileName; set => this.RaiseAndSetIfChanged(ref assetsFileName, value); }

    private bool canPickAsset = false;
    public bool CanPickAsset { get => canPickAsset; set => this.RaiseAndSetIfChanged(ref canPickAsset, value); }
    private bool canGoToViewer = false;
    public bool CanGoToViewer { get => canGoToViewer; set => this.RaiseAndSetIfChanged(ref canGoToViewer, value); }
    private bool canNotGoToViewer = true;
    public bool CanNotGoToViewer { get => canNotGoToViewer; set => this.RaiseAndSetIfChanged(ref canNotGoToViewer, value); }

    //EditorGreeter
    private string? editedError = null;
    public string? EditedError { get => editedError; set => this.RaiseAndSetIfChanged(ref editedError, value); }
    private string? editedFileName = null;
    public string? EditedFileName { get => editedFileName; set => this.RaiseAndSetIfChanged(ref editedFileName, value); }

    private bool canGoToEditor = false;
    public bool CanGoToEditor { get => canGoToEditor; set => this.RaiseAndSetIfChanged(ref canGoToEditor, value); }
    private bool canNotGoToEditor = true;
    public bool CanNotGoToEditor { get => canNotGoToEditor; set => this.RaiseAndSetIfChanged(ref canNotGoToEditor, value); }

    readonly FilePickerFileType JsonFile = new("Json Files")
    {
        Patterns = ["*.json"],
        AppleUniformTypeIdentifiers = ["public.json"],
        MimeTypes = ["application/json", "text/json"]
    };

    public GreeterViewModel(Arguments arguments)
    {
        H1Size = BaseSize * 0.75;
        LargeImageSize = BaseSize * 5;
        Arguments = arguments;
        if (Arguments.EditPath != null)
        {
            try
            {
                SourceData = new(File.ReadAllText(arguments.EditPath!));
            }
            catch
            {
                SourceData = null;
                try
                {
                    Assets = new(File.ReadAllText(arguments.EditPath!));
                }
                catch
                {
                    Assets = null;
                }
            }

            if (Assets != null) CurrentPage = new AssetsEditorView();
            if (SourceData != null) CurrentPage = new SourceDataEditorView();
        }

        if (Arguments.DataPath != null || Arguments.AssetsPath != null)
        {
            try
            {
                SourceData = new(File.ReadAllText(arguments.DataPath!));
            }
            catch
            {
                SourceData = null;
                try
                {
                    ResultData = new(File.ReadAllText(arguments.DataPath!));
                }
                catch
                {
                    ResultData = null;
                }
            }

            try
            {
                Assets = new(File.ReadAllText(arguments.AssetsPath!));
            }
            catch
            {
                Assets = null;
            }

            if (SourceData != null && SourceData.Data.Count > 0 && Assets != null && Assets.Assets!.Count > 0)
            {
                ResultData = new();
                Optimizer.GetResult(Assets, SourceData, ResultData);
            }

            if (ResultData != null) CurrentPage = new ViewerGraphListView() { DataContext = new ViewerViewModel(BaseSize, ResultData) };
        }
    }

    public void GoToViewerGreeter() => CurrentPage = new ViewerGreeterView();

    public void GoToEditorGreeter() => CurrentPage = new EditorGreeterView();
    public void GoToEditorTypeGreeter() => CurrentPage = new EditorTypeGreeterView();

    public void GoToViewer()
    {
        if (ResultData == null)
        {
            ResultData = new();
            Optimizer.GetResult(Assets!, SourceData!, ResultData);
        }
        CurrentPage = new ViewerGraphListView
        {
            DataContext = new ViewerViewModel(BaseSize, ResultData!)
        };
    }

    public void GoToEditor()
    {
        if (SourceData != null)
            CurrentPage = new SourceDataEditorView() { DataContext = new SourceDataEditorViewModel(BaseSize, SourceData) };
        if (Assets != null)
            CurrentPage = new AssetsEditorView() { DataContext = new AssetsEditorViewModel(BaseSize, Assets) };
    }

    public void CreateAssetsEditor() { Assets = new(); GoToEditor(); }
    public void CreateSourceDataEditor() { SourceData = new(); GoToEditor(); }

    public async void LoadDataFile()
    {
        // Start async operation to open the dialog.
        var files = await App.TopLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Open a source or result data json file",
            AllowMultiple = false,
            FileTypeFilter = [JsonFile]
        });

        if (files.Count >= 1)
        {
            SourceData = null;
            ResultData = null;
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

            if (text != null)
                try
                {
                    SourceData = new(text);
                }
                catch
                {
                    try
                    {
                        ResultData = new(text);
                    }
                    catch
                    {
                        DataError = "Data file contains invalid json";
                    }
                }
            if (SourceData != null)
            {
                if (SourceData.Data.Count > 0)
                {
                    DataFileName = files[0].Name;
                }
                else
                {
                    Assets = null;
                    DataError = "Data file is empty";
                }
            }
            if (ResultData != null)
            {
                if (ResultData.Data.Count > 0)
                {
                    DataFileName = files[0].Name;
                }
                else
                {
                    Assets = null;
                    DataError = "Data file is empty";
                }
            }
        }

        CanPickAsset = SourceData != null;
        CanGoToViewer = ResultData != null || SourceData != null && Assets != null;
        CanNotGoToViewer = !CanGoToViewer;
    }
    public async void LoadAssetsFile()
    {
        // Start async operation to open the dialog.
        var files = await App.TopLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Open an assets json file",
            AllowMultiple = false,
            FileTypeFilter = [JsonFile]
        });

        if (files.Count >= 1)
        {
            Assets = null;
            AssetsFileName = null;
            AssetsError = null;
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
                AssetsError = "Couldn't read file";
            }

            if (text != null)
                try
                {
                    Assets = new(text);
                }
                catch
                {
                    AssetsError = "File contains invalid json";
                }

            if (Assets != null)
            {
                if (Assets.Assets!.Count > 0)
                {
                    AssetsFileName = files[0].Name;
                }
                else
                {
                    Assets = null;
                    AssetsError = "Assets file is empty";
                }
            }
        }

        CanGoToViewer = ResultData != null || SourceData != null && Assets != null;
        CanNotGoToViewer = !CanGoToViewer;
    }
    public async void LoadEditedFile()
    {
        // Start async operation to open the dialog.
        var files = await App.TopLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Open an assets or source data json file",
            AllowMultiple = false,
            FileTypeFilter = [JsonFile]
        });

        if (files.Count >= 1)
        {
            SourceData = null;
            Assets = null;
            DataFileName = null;
            EditedError = null;
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
                EditedError = "Couldn't read file";
            }

            if (text != null)
                try
                {
                    SourceData = new(text);
                }
                catch
                {
                    try
                    {
                        Assets = new(text);
                    }
                    catch
                    {
                        EditedError = "Edited file contains invalid json";
                    }
                }
            if (SourceData != null)
            {

                EditedFileName = files[0].Name;
            }
            if (Assets != null)
            {
                EditedFileName = files[0].Name;
            }
        }
        else
        {
            EditedError = "Invalid file selection";
        }

        CanGoToEditor = Assets != null || SourceData != null;
        CanNotGoToEditor = !CanGoToEditor;
    }
}