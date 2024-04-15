using Avalonia.Controls;
namespace HeatManagement.ViewModels;

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
}