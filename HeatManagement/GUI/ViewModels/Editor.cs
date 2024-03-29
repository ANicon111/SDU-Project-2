using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace HeatManagement.GUI;

class EditorViewModel : ViewModelBase
{
    readonly string Mode;
    readonly SourceDataManager? SourceData;
    readonly AssetManager? Assets;

    ObservableCollection<string> elements = [];

    public EditorViewModel(SourceDataManager sourceData)
    {
        Mode = "source";
        SourceData = sourceData;
        Assets = null;

        elements = [Mode];//TODO
    }

    public EditorViewModel(AssetManager assets)
    {
        Mode = "source";
        SourceData = null;
        Assets = assets;

        elements = [Mode];
    }
}