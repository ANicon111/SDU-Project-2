using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using ReactiveUI;

namespace HeatManagement.GUI;

class ViewerViewModel : ViewModelBase
{
    readonly ResultDataManager ResultData;

    //derived data
    readonly Dictionary<string, Dictionary<Tuple<DateTime, DateTime>, double>> GraphValues;
    readonly Tuple<DateTime, DateTime>[] Times;
    readonly string[] Measurements;
    readonly string[] Options;

    //GraphList
    readonly List<SolidColorBrush> ColorList = [];
    private readonly ObservableCollection<Button> graphButtonList = [];
    public ObservableCollection<Button> GraphButtonList { get => graphButtonList; }
    readonly List<int> SelectedGraphList = [];
    private bool graphOpen = false;
    public bool GraphOpen { get => graphOpen; set => this.RaiseAndSetIfChanged(ref graphOpen, value); }
    private bool canOpenGraph = false;
    public bool CanOpenGraph { get => canOpenGraph; set => this.RaiseAndSetIfChanged(ref canOpenGraph, value); }
    private ViewerGraphView? graph = null;
    public ViewerGraphView? Graph { get => graph; set => this.RaiseAndSetIfChanged(ref graph, value); }

    //Graph
    public double BaseSize { get; } = MainWindow.FontSize * 2;
    private Point zeroLineLength = new(0, 0);
    public Point ZeroLineLength { get => zeroLineLength; set => this.RaiseAndSetIfChanged(ref zeroLineLength, value); }
    public ObservableCollection<ViewerTimeButtonViewModel> GraphBarList { get; } = [];
    public ObservableCollection<ViewerTextViewModel> GraphValueList { get; } = [];
    private string graphTime = "";
    public string GraphTime { get => graphTime; set => this.RaiseAndSetIfChanged(ref graphTime, value); }

    //Locals
    int LastSelectedIndex = 0;
    public ReactiveCommand<int, Unit> GraphButtonClick;

    public ViewerViewModel(ResultDataManager resultData)
    {
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

        GraphValues = new(){
            {"Cost", []},
            {"Electricity", []},
            {"CO2", []},
        };

        //get individual asset usage stats
        foreach (var result in ResultData.Data)
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
                    assetOptions.Add($"{assetName}-Electricity");
                    GraphValues.Add($"{assetName}-Electricity", []);
                    assetMeasurements.Add("MWh");
                    assetOptions.Add($"{assetName}-CO2");
                    GraphValues.Add($"{assetName}-CO2", []);
                    assetMeasurements.Add("kg");
                }

                //set the hardcoded asset values
                GraphValues[$"{assetName}-Cost"][result.Key] = assetResult.Value.Cost;
                GraphValues[$"{assetName}-Electricity"][result.Key] = assetResult.Value.Electricity;
                GraphValues[$"{assetName}-CO2"][result.Key] = assetResult.Value.CO2;

                //get additional resource usage
                foreach (KeyValuePair<string, double> additionalResource in assetResult.Value.AdditionalResources)
                {
                    //format resource name
                    string resourceName = additionalResource.Key[..1].ToUpper() + additionalResource.Key[1..].ToLower();

                    //initialize and add resource value
                    if (!resourceOptions.Contains(resourceName))
                    {
                        resourceOptions.Add(resourceName);
                        GraphValues.Add(resourceName, []);
                        resourceMeasurements.Add("MWh");
                    }
                    if (!GraphValues[resourceName].ContainsKey(result.Key)) GraphValues[resourceName][result.Key] = 0;
                    GraphValues[resourceName][result.Key] += additionalResource.Value;

                    //initialize and set asset resource usage
                    if (!assetOptions.Contains($"{assetName}-{resourceName}"))
                    {
                        assetOptions.Add($"{assetName}-{resourceName}");
                        GraphValues.Add($"{assetName}-{resourceName}", []);
                        assetMeasurements.Add("MWh");
                    }
                    GraphValues[$"{assetName}-{resourceName}"][result.Key] = additionalResource.Value;
                }
            }
        }

        //merge options and units of measurements
        Options = [.. resourceOptions, .. assetOptions];
        Measurements = [.. resourceMeasurements, .. assetMeasurements];

        //golden ratio-based color generation
        for (int i = 0; i < Options.Length; i++) ColorList.Add(new SolidColorBrush(HslColor.FromHsl(137.5 * i % 360, 1, 1.618 * i % 0.25 + 0.33).ToRgb()));

        //button command
        GraphButtonClick = ReactiveCommand.Create((int index) =>
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
                GraphButtonList[i].Background = selected ? ColorList[i] : null;
                GraphButtonList[i].Foreground = selected ? new SolidColorBrush(Colors.White) : ColorList[i];
                GraphButtonList[i].IsVisible = selectable;
            }
            CanOpenGraph = SelectedGraphList.Count > 0;
        });

        //initialize button list
        for (int i = 0; i < Options.Length; i++)
        {
            GraphButtonList.Add(new()
            {
                Content = Options[i],
                Foreground = ColorList[i],
                CommandParameter = i
            });
            GraphButtonList[i].Command = GraphButtonClick;
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

    public void SelectBar(int index)
    {
        GraphBarList[LastSelectedIndex].BarBackground = LastSelectedIndex % 2 == 0 ? new SolidColorBrush(0x20303030) : new SolidColorBrush(0x20101010);
        LastSelectedIndex = index;
        GraphBarList[index].BarBackground = new SolidColorBrush(0x20ffffff);
        GraphTime = $"{Times[index].Item1:dd'.'MM'.'yyyy' 'HH':'mm':'ss} - {Times[index].Item2:dd'.'MM'.'yyyy' 'HH':'mm':'ss}";
        for (int i = 0; i < SelectedGraphList.Count; i++)
        {
            GraphValues[Options[SelectedGraphList[i]]].TryGetValue(Times[index], out double value);
            string measurement = Measurements[SelectedGraphList[i]];
            string name = Options[SelectedGraphList[i]];
            GraphValueList[i].Text = $"{name}: {value:0.##} {measurement}";
        }
        this.RaisePropertyChanged(nameof(GraphValueList));
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
            GraphValueList.Add(new("", colors[j]));
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

class ViewerTextViewModel(string value, SolidColorBrush color) : ViewModelBase
{
    private string text = value;
    public string Text { get => text; set => this.RaiseAndSetIfChanged(ref text, value); }
    private readonly SolidColorBrush color = color;
    public SolidColorBrush Color { get => color; }
}
