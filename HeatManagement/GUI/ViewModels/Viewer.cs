using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.Json;
using Avalonia;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Platform.Storage;
using ReactiveUI;

namespace HeatManagement.GUI;

class ViewerViewModel : ViewModelBase
{
    readonly ResultDataManager ResultData;

    //derived data
    readonly Dictionary<string, Dictionary<Tuple<DateTime, DateTime>, double>> GraphValues;
    readonly Tuple<DateTime, DateTime>[] Times;
    readonly string[] ImagePaths;
    readonly string[] Measurements;
    readonly string[] Options;

    //GraphList
    public double BaseSize { get; } = MainWindow.FontSize * 2;
    public double TitleSize { get; } = MainWindow.FontSize * 3;
    public double H1Size { get; } = MainWindow.FontSize * 1.5;
    readonly List<SolidColorBrush> ColorList = [];
    private readonly ObservableCollection<GraphButtonViewModel> graphButtonList = [];
    public ObservableCollection<GraphButtonViewModel> GraphButtonList { get => graphButtonList; }
    readonly List<int> SelectedGraphList = [];
    private bool graphOpen = false;
    public bool GraphOpen { get => graphOpen; set => this.RaiseAndSetIfChanged(ref graphOpen, value); }
    private bool canOpenGraph = false;
    public bool CanOpenGraph { get => canOpenGraph; set { this.RaiseAndSetIfChanged(ref canOpenGraph, value); this.RaisePropertyChanged(nameof(CanNotOpenGraph)); } }
    public bool CanNotOpenGraph { get => !canOpenGraph; }
    private ViewerGraphView? graph = null;
    public ViewerGraphView? Graph { get => graph; set => this.RaiseAndSetIfChanged(ref graph, value); }

    //Graph
    private Point zeroLineLength = new(0, 0);
    public Point ZeroLineLength { get => zeroLineLength; set => this.RaiseAndSetIfChanged(ref zeroLineLength, value); }
    public ObservableCollection<ViewerTimeButtonViewModel> GraphBarList { get; } = [];
    public ObservableCollection<ViewerTextViewModel> GraphValueList { get; } = [];
    private string graphTime = "";
    public string GraphTime { get => graphTime; set => this.RaiseAndSetIfChanged(ref graphTime, value); }

    int LastSelectedIndex = 0;
    public void SelectBar(int index)
    {
        GraphBarList[LastSelectedIndex].BarBackground = LastSelectedIndex % 2 == 0 ? new SolidColorBrush(0x20303030) : new SolidColorBrush(0x20101010);
        LastSelectedIndex = index;
        GraphBarList[index].BarBackground = new SolidColorBrush(0x20ffffff);
        GraphTime = $"{Times[index].Item1:yyyy'-'MM'-'dd' 'HH':'mm':'ss}\n{Times[index].Item2:yyyy'-'MM'-'dd' 'HH':'mm':'ss}";
        for (int i = 0; i < SelectedGraphList.Count; i++)
        {
            GraphValues[Options[SelectedGraphList[i]]].TryGetValue(Times[index], out double value);
            string measurement = Measurements[SelectedGraphList[i]];
            string name = Options[SelectedGraphList[i]];
            GraphValueList[i].Text = $"{name}: {value:0.##} {measurement}";
        }
        this.RaisePropertyChanged(nameof(GraphValueList));
    }

    public void SelectGraph(int index)
    {

        if (!SelectedGraphList.Remove(index))
            SelectedGraphList.Add(index);

        for (int i = 0; i < GraphButtonList.Count; i++)
        {
            bool selected = SelectedGraphList.Contains(i);
            bool selectable;
            if (SelectedGraphList.Count > 0)
                selectable = Measurements[SelectedGraphList[0]] == Measurements[i];
            else
                selectable = true;
            GraphButtonList[i].Background = selectable ? selected ? ColorList[i] : new SolidColorBrush(0x10191919) : new SolidColorBrush(0x10aaaaaa);
            GraphButtonList[i].Foreground = selectable ? selected ? new SolidColorBrush(Colors.White) : ColorList[i] : new SolidColorBrush(0xffaaaaaa);
            GraphButtonList[i].IsEnabled = selectable;
        }
        CanOpenGraph = SelectedGraphList.Count > 0;
    }

    public ViewerViewModel(double baseSize, ResultDataManager resultData)
    {
        BaseSize = baseSize;
        TitleSize = BaseSize * 1.5;
        H1Size = BaseSize * 0.75;
        ResultData = resultData;
        Times = [.. ResultData!.Data.Keys];

        List<string> resourceOptions = [
            "Cost",
            "Electricity",
            "CO2",
        ];

        List<string> assetOptions = [];

        List<string> resourceMeasurements =
        [
            "dkk",
            "MWh",
            "kg",
        ];

        List<string> assetMeasurements = [];

        List<string> resourceImagePaths = [
            "",
            "",
            "",
        ];

        List<string> assetImagePaths = [];

        GraphValues = new(){
            {"Cost", []},
            {"Electricity", []},
            {"CO2", []},
        };

        //get individual asset usage stats
        foreach (KeyValuePair<Tuple<DateTime, DateTime>, Dictionary<string, ResultData>> result in ResultData.Data)
        {
            GraphValues["Cost"][result.Key] = 0;
            GraphValues["Electricity"][result.Key] = 0;
            GraphValues["CO2"][result.Key] = 0;

            foreach (KeyValuePair<string, ResultData> assetResult in result.Value)
            {
                //format asset name
                string assetName = assetResult.Key.ToUpper();

                //add hardcoded values
                GraphValues["Cost"][result.Key] += assetResult.Value.Cost;
                GraphValues["Electricity"][result.Key] += assetResult.Value.Electricity;
                GraphValues["CO2"][result.Key] += assetResult.Value.CO2;

                //initialize hardcoded asset values
                if (!GraphValues.ContainsKey($"{assetName}-Cost"))
                {
                    assetOptions.Add($"{assetName}-Cost");
                    GraphValues.Add($"{assetName}-Cost", []);
                    assetMeasurements.Add("dkk");
                    assetImagePaths.Add(ResultData.Assets[assetName].ImagePath);

                    assetOptions.Add($"{assetName}-Electricity");
                    GraphValues.Add($"{assetName}-Electricity", []);
                    assetMeasurements.Add("MWh");
                    assetImagePaths.Add(ResultData.Assets[assetName].ImagePath);

                    assetOptions.Add($"{assetName}-CO2");
                    GraphValues.Add($"{assetName}-CO2", []);
                    assetMeasurements.Add("kg");
                    assetImagePaths.Add(ResultData.Assets[assetName].ImagePath);
                }

                //set the hardcoded asset values
                GraphValues[$"{assetName}-Cost"][result.Key] = assetResult.Value.Cost;
                GraphValues[$"{assetName}-Electricity"][result.Key] = assetResult.Value.Electricity;
                GraphValues[$"{assetName}-CO2"][result.Key] = assetResult.Value.CO2;

                //get additional resource usage
                foreach (KeyValuePair<string, AdditionalResource> additionalResource in assetResult.Value.AdditionalResources)
                {
                    //format resource name
                    string resourceName = additionalResource.Key[..1].ToUpper() + additionalResource.Key[1..].ToLower();

                    //initialize and add resource value
                    if (!resourceOptions.Contains(resourceName))
                    {
                        resourceOptions.Add(resourceName);
                        GraphValues.Add(resourceName, []);
                        resourceMeasurements.Add(additionalResource.Value.Measurement);
                        resourceImagePaths.Add("");
                    }
                    if (!GraphValues[resourceName].ContainsKey(result.Key)) GraphValues[resourceName][result.Key] = 0;
                    GraphValues[resourceName][result.Key] += additionalResource.Value.Value;

                    //initialize and set asset resource usage
                    if (!assetOptions.Contains($"{assetName}-{resourceName}"))
                    {
                        assetOptions.Add($"{assetName}-{resourceName}");
                        GraphValues.Add($"{assetName}-{resourceName}", []);
                        assetMeasurements.Add(additionalResource.Value.Measurement);
                        assetImagePaths.Add(ResultData.Assets[assetName].ImagePath);
                    }
                    GraphValues[$"{assetName}-{resourceName}"][result.Key] = additionalResource.Value.Value;
                }
            }
        }

        //merge options and units of measurements
        Options = [.. resourceOptions, .. assetOptions];
        Measurements = [.. resourceMeasurements, .. assetMeasurements];
        ImagePaths = [.. resourceImagePaths, .. assetImagePaths];

        //golden ratio-based color generation
        for (int i = 0; i < Options.Length; i++) ColorList.Add(new SolidColorBrush(HslColor.FromHsl(137.5 * i % 360, 1, 1.618 * i % 0.25 + 0.33).ToRgb()));

        //initialize button list
        for (int i = 0; i < Options.Length; i++)
        {
            GraphButtonList.Add(new(ImagePaths[i], Options[i].Split('-').Last(), ColorList[i], SelectGraph, i, BaseSize)
            {
                Content = Options[i]
            });
        }
    }

    public void ToggleGraph()
    {
        if (!GraphOpen)
        {
            //I have to memory leak my way to refresh the top of the graph page
            Graph = new();
            LoadGraphs();
        }
        GraphOpen = !GraphOpen;
    }

    public void LoadGraphs()
    {
        ZeroLineLength = new(BaseSize * Times.Length * SelectedGraphList.Count, 0);
        double[][] values = new double[Times.Length][];
        SolidColorBrush[] colors = new SolidColorBrush[Times.Length];
        double min = -1e-10;
        double max = 1e-10;

        //get values
        for (int i = 0; i < Times.Length; i++)
        {
            values[i] = new double[SelectedGraphList.Count];
            for (int j = 0; j < SelectedGraphList.Count; j++)
            {
                values[i][j] = 0;
                if (GraphValues[Options[SelectedGraphList[j]]].TryGetValue(Times[i], out double value)) values[i][j] = value;
                if (values[i][j] < min) min = values[i][j];
                if (values[i][j] > max) max = values[i][j];
            }
        }

        //get colors and add graph values with said colors
        GraphValueList.Clear();
        for (int j = 0; j < SelectedGraphList.Count; j++)
        {
            colors[j] = ColorList[SelectedGraphList[j]];
            GraphValueList.Add(new("", colors[j], H1Size));
        }

        //normalize the values to between -1 and 1 and add graphs
        GraphBarList.Clear();
        double normalizationRatio = 1 / Math.Max(max, -min);
        for (int i = 0; i < Times.Length; i++)
        {
            for (int j = 0; j < SelectedGraphList.Count; j++)
            {
                values[i][j] *= normalizationRatio;
            }
            GraphBarList.Add(new(BaseSize, values[i], colors, i, SelectBar));
        }
        SelectBar(0);
    }

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
    public async void ExportResultData()
    {
        // Start async operation to open the dialog.
        IStorageFile? file = await App.TopLevel.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            Title = "Export result data",
            SuggestedFileName = "resultData.json",
            FileTypeChoices = [JsonFile]
        });

        if (file != null)
        {
            try
            {
                // Open writing stream from the file.
                await using Stream stream = await file.OpenWriteAsync();
                using StreamWriter streamWriter = new(stream);
                // Writes all the content of file as a text.
                await streamWriter.WriteAsync(ResultData.ToJson());
            }
            catch { }
        }
    }

    private struct JsonData(DateTime startTime, DateTime endTime, SortedDictionary<string, double> values)
    {
        public DateTime StartTime { get; set; } = startTime;
        public DateTime EndTime { get; set; } = endTime;
        public SortedDictionary<string, double> Values { get; set; } = values;
    }
    private readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };
    public async void ExportSelectedGraphs()
    {
        // Start async operation to open the dialog.
        IStorageFile? file = await App.TopLevel.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            Title = "Export graph data",
            SuggestedFileName = "graph.csv",
            FileTypeChoices = [CSVFile, JsonFile]
        });

        if (file != null)
        {
            //create the json exportable dictionary and the csv table
            List<string> csvHeader = ["StartTime", "EndTime"];
            for (int j = 0; j < SelectedGraphList.Count; j++) csvHeader.Add(Options[SelectedGraphList[j]]);
            List<string[]> csvTable = [[.. csvHeader]];
            List<JsonData> jsonValues = [];

            for (int i = 0; i < Times.Length; i++)
            {
                jsonValues.Add(new(Times[i].Item1, Times[i].Item2, []));

                string[] csvRow = new string[csvHeader.Count];
                csvRow[0] = $"{Times[i].Item1:s}";
                csvRow[1] = $"{Times[i].Item2:s}";
                for (int j = 0; j < SelectedGraphList.Count; j++)
                {
                    GraphValues[Options[SelectedGraphList[j]]].TryGetValue(Times[i], out double value);
                    jsonValues[i].Values.Add(Options[SelectedGraphList[j]], value);
                    csvRow[2 + j] = $"{value}";
                }
                csvTable.Add(csvRow);
            }

            try
            {
                // Open writing stream from the file.
                await using Stream stream = await file.OpenWriteAsync();
                using StreamWriter streamWriter = new(stream);
                // Writes all the content of file as a text.
                if (file.Name.Split('.').Last().ToLower() == "json")
                    await streamWriter.WriteAsync(JsonSerializer.Serialize(jsonValues, JsonOptions));
                else
                    await streamWriter.WriteAsync(Utils.TableToCSV(csvTable));
            }
            catch { }
        }
    }
}

class GraphButtonViewModel : ViewModelBase
{
    private Bitmap? asset;
    private Bitmap? resource;
    private SolidColorBrush? background;
    private SolidColorBrush? foreground;
    private string? content;
    private bool isEnabled = true;
    private readonly Action<int> GraphButtonClick;
    private readonly int Index;

    public Bitmap? Asset { get => asset; set => this.RaiseAndSetIfChanged(ref asset, value); }
    public Bitmap? Resource { get => resource; set => this.RaiseAndSetIfChanged(ref resource, value); }
    public SolidColorBrush? Background { get => background; set => this.RaiseAndSetIfChanged(ref background, value); }
    public SolidColorBrush? Foreground { get => foreground; set => this.RaiseAndSetIfChanged(ref foreground, value); }
    public string? Content { get => content; set => this.RaiseAndSetIfChanged(ref content, value); }
    public double BaseSize { get; }
    public double LargeImageSize { get; }
    public double SmallImageSize { get; }
    public double SmallImageMargin { get; }
    public void Command()
    {
        GraphButtonClick(Index);
    }
    public bool IsEnabled { get => isEnabled; set => this.RaiseAndSetIfChanged(ref isEnabled, value); }
    public GraphButtonViewModel(string imagePath, string resourceName, SolidColorBrush accentColor, Action<int> graphButtonClick, int index, double baseSize)
    {
        BaseSize = baseSize;
        LargeImageSize = BaseSize * 5;
        SmallImageSize = BaseSize * 3;
        SmallImageMargin = BaseSize / 4;
        resourceName = resourceName.ToLower();
        try
        {
            resource = new Bitmap(AssetLoader.Open(new Uri($"avares://HeatManagement/GUI/Assets/Resources/{resourceName}.png")));
        }
        catch
        {
            resource = new Bitmap(AssetLoader.Open(new Uri($"avares://HeatManagement/GUI/Assets/Resources/unknown.png")));
        }

        if (imagePath != "")
        {
            try
            {
                if (File.Exists(imagePath))
                {
                    asset = new Bitmap(File.OpenRead(imagePath));
                }
            }
            catch { }
        }

        if (asset == null && resource != null)
        {
            asset = resource;
            resource = null;
        }

        foreground = accentColor;
        GraphButtonClick = graphButtonClick;
        Index = index;
    }
}

class ViewerTimeButtonViewModel : ViewModelBase
{
    public double BaseSize { get; }
    public double BarHeight { get; }
    public double BarWidth { get; }
    private ObservableCollection<ViewerBarViewModel> bars;
    public ObservableCollection<ViewerBarViewModel> Bars { get => bars; set => this.RaiseAndSetIfChanged(ref bars, value); }
    private SolidColorBrush barBackground;
    public SolidColorBrush BarBackground { get => barBackground; set => this.RaiseAndSetIfChanged(ref barBackground, value); }
    private readonly double[] Values;

    readonly int Index;
    readonly Action<int> selectBar;

    public void SelectBar() => selectBar(Index);

    public ViewerTimeButtonViewModel(double baseSize, double[] values, SolidColorBrush[] colors, int index, Action<int> selectBar)
    {
        BaseSize = baseSize;
        BarHeight = baseSize * 2;
        Values = values;
        bars = [];
        Index = index;
        this.selectBar = selectBar;
        barBackground = Index % 2 == 0 ? new SolidColorBrush(0x20303030) : new SolidColorBrush(0x20101010);

        for (int i = 0; i < values.Length; i++)
        {
            Bars.Add(new(baseSize, Values[i], colors[i]));
        }
        BarWidth = Bars.Count * baseSize;

    }
}

class ViewerBarViewModel : ViewModelBase
{
    public double BaseSize { get; }
    private double height;
    public double Height { get => height; set => this.RaiseAndSetIfChanged(ref height, value); }
    private SolidColorBrush color = new();
    public SolidColorBrush Color { get => color; set => this.RaiseAndSetIfChanged(ref color, value); }
    private string barSignAlignment = "Top";
    public string BarSignAlignment { get => barSignAlignment; set => this.RaiseAndSetIfChanged(ref barSignAlignment, value); }
    private string barAlignment = "Top";
    public string BarAlignment { get => barAlignment; set => this.RaiseAndSetIfChanged(ref barAlignment, value); }

    public ViewerBarViewModel(double baseSize, double height, SolidColorBrush color)
    {
        BaseSize = baseSize;
        Height = Math.Abs(height) * baseSize;
        if (height > 0)
        {
            BarSignAlignment = "Top";
            BarAlignment = "Bottom";
        }
        else
        {
            BarSignAlignment = "Bottom";
            BarAlignment = "Top";
        }
        Color = color;
    }
}

class ViewerTextViewModel(string value, SolidColorBrush color, double fontSize) : ViewModelBase
{
    private string text = value;
    public string Text { get => text; set => this.RaiseAndSetIfChanged(ref text, value); }
    public SolidColorBrush Color { get; } = color;
    public double FontSize { get; } = fontSize;
}
