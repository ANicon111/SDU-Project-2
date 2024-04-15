using Avalonia.Controls;
namespace HeatManagement.ViewModels;

using System.IO;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using HeatManagement.Views;
using ReactiveUI;

class ViewModelBase: ReactiveObject;

class MainWindowViewModel: ViewModelBase{
    private UserControl currentPage = new Greeter();
    public UserControl CurrentPage { get => currentPage; set => this.RaiseAndSetIfChanged(ref currentPage, value); }
    
    public void GoToViewer(){
        CurrentPage = new ViewerGreeter();
    }

    public void GoToEditor(){
        CurrentPage = new EditorGreeter();
    }

    public string? ReadFile()
    {
        try{
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
        }catch{}
        return null;
    }

    AssetManager assetManager = new();
    SourceDataManager sourceDataManager = new();
    ResultDataManager resultDataManager = new();

    public void OpenAssetOrSourceFile()
    {
        string? fileContent=ReadFile();

        if(fileContent == null){
            return;
        }

        try{
            sourceDataManager.JsonImport(fileContent!);
        }catch{
            try{
                assetManager.JsonImport(fileContent!);
            }catch{
                return;
            }
        }
    }

}