using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reactive;
using Avalonia.Controls;
using Avalonia.Media;
using ReactiveUI;

namespace HeatManagement.GUI;

class ViewerViewModel : ViewModelBase
{
    readonly ResultDataManager ResultData;

    readonly Dictionary<string, Dictionary<Tuple<DateTime, DateTime>, double>> GraphValues;
    readonly Tuple<DateTime, DateTime>[] Times;
    readonly string[] Measurements;
    readonly string[] options;

    private readonly ObservableCollection<Button> graphButtonList = [];
    public ObservableCollection<Button> GraphButtonList { get => graphButtonList; }
    readonly List<int> selectedGraphList = [];
    readonly List<SolidColorBrush> colorList = [];

    public ReactiveCommand<int, Unit> GraphButtonClick;

    public ViewerViewModel(ResultDataManager resultData)
    {
        ResultData = resultData;
        List<string> resourceOptions = [
            "Cost",
            "Electricity",
            "CO2",
        ];

        List<string> assetOptions = [];


        Times = [.. ResultData!.Data.Keys];

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
        options = [.. resourceOptions, .. assetOptions];
        Measurements = [.. resourceMeasurements, .. assetMeasurements];

        //golden ratio-based color generation
        for (int i = 0; i < options.Length; i++) colorList.Add(new SolidColorBrush(HslColor.FromHsl(137.5 * i % 360, 1, 1.618 * i % 0.25 + 0.33).ToRgb()));


        //button command
        GraphButtonClick = ReactiveCommand.Create((int index) =>
        {
            if (!selectedGraphList.Remove(index))
                selectedGraphList.Add(index);

            for (int i = 0; i < GraphButtonList.Count; i++)
            {
                bool selected = selectedGraphList.Contains(i);
                bool selectable;
                if (selectedGraphList.Count > 0)
                    selectable = Measurements[selectedGraphList[0]] == Measurements[i];
                else
                    selectable = true;
                GraphButtonList[i].Background = selected ? colorList[i] : null;
                GraphButtonList[i].Foreground = selected ? new SolidColorBrush(Colors.White) : colorList[i];
                GraphButtonList[i].IsVisible = selectable;
            }
            CanOpenGraph = selectedGraphList.Count > 0;
        });

        //initialize button list
        for (int i = 0; i < options.Length; i++)
        {
            GraphButtonList.Add(new()
            {
                Content = options[i],
                Foreground = colorList[i],
                CommandParameter = i
            });
            GraphButtonList[i].Command = GraphButtonClick;
        }
    }

    private bool graphOpen = false;
    public bool GraphOpen { get => graphOpen; set => this.RaiseAndSetIfChanged(ref graphOpen, value); }
    private ViewerGraphView? graph = null;
    public ViewerGraphView? Graph { get => graph; set => this.RaiseAndSetIfChanged(ref graph, value); }

    private bool canOpenGraph = false;
    public bool CanOpenGraph { get => canOpenGraph; set => this.RaiseAndSetIfChanged(ref canOpenGraph, value); }

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

    public ObservableCollection<ViewerBarViewModel> GraphBarList { get; } = [];
    public ObservableCollection<StringViewModel> GraphValueList { get; } = [];
    private string graphTime = "";
    public string GraphTime { get => graphTime; set => this.RaiseAndSetIfChanged(ref graphTime, value); }

    int lastSelectedIndex = 0;

    public void SelectBar(int index)
    {
        GraphBarList[lastSelectedIndex].BarBackground = lastSelectedIndex % 2 == 0 ? new SolidColorBrush(0x20303030) : new SolidColorBrush(0x20101010);
        lastSelectedIndex = index;
        GraphBarList[index].BarBackground = new SolidColorBrush(0x20ffffff);
        GraphTime = $"{Times[index].Item1:dd'.'MM'.'yyyy' 'HH':'mm':'ss} - {Times[index].Item2:dd'.'MM'.'yyyy' 'HH':'mm':'ss}";
        for (int i = 0; i < selectedGraphList.Count; i++)
        {
            GraphValues[options[selectedGraphList[i]]].TryGetValue(Times[index], out double value);
            string measurement = Measurements[selectedGraphList[i]];
            string name = options[selectedGraphList[i]];
            GraphValueList[i].Text = $"{name}: {value:0.##} {measurement}";
        }
        this.RaisePropertyChanged(nameof(GraphValueList));
    }

    public void LoadGraphs()
    {
        double[][] values = new double[Times.Length][];
        SolidColorBrush[] colors = new SolidColorBrush[Times.Length];
        double min = -1e-10;
        double max = 1e-10;

        //get values
        for (int i = 0; i < Times.Length; i++)
        {
            values[i] = new double[selectedGraphList.Count];
            for (int j = 0; j < selectedGraphList.Count; j++)
            {
                values[i][j] = 0;
                if (GraphValues[options[selectedGraphList[j]]].TryGetValue(Times[i], out double value)) values[i][j] = value;
                if (values[i][j] < min) min = values[i][j];
                if (values[i][j] > max) max = values[i][j];
            }
        }

        //get colors and add graph values with said colors
        GraphValueList.Clear();
        for (int j = 0; j < selectedGraphList.Count; j++)
        {
            colors[j] = colorList[selectedGraphList[j]];
            GraphValueList.Add(new("", colors[j]));
        }

        //normalize the values to between -1 and 1 and add graphs
        GraphBarList.Clear();
        double normalizationRatio = 1 / Math.Max(max, -min);
        for (int i = 0; i < Times.Length; i++)
        {
            for (int j = 0; j < selectedGraphList.Count; j++)
            {
                values[i][j] *= normalizationRatio;
            }
            GraphBarList.Add(new(values[i], colors, i, SelectBar));
        }
        SelectBar(0);
    }
}

class ViewerBarViewModel : ViewModelBase
{
    private ObservableCollection<BarViewModel> bars;
    public ObservableCollection<BarViewModel> Bars { get => bars; set => this.RaiseAndSetIfChanged(ref bars, value); }
    private double barWidth = 0;
    public double BarWidth { get => barWidth; set => this.RaiseAndSetIfChanged(ref barWidth, value); }
    private SolidColorBrush barBackground;
    public SolidColorBrush BarBackground { get => barBackground; set => this.RaiseAndSetIfChanged(ref barBackground, value); }
    private readonly double[] Values;

    readonly int Index;
    readonly Action<int> selectBar;

    public void SelectBar() => selectBar(Index);


    public ViewerBarViewModel(double[] values, SolidColorBrush[] colors, int index, Action<int> selectBar)
    {
        Values = values;
        bars = [];
        Index = index;
        this.selectBar = selectBar;
        barBackground = Index % 2 == 0 ? new SolidColorBrush(0x20303030) : new SolidColorBrush(0x20101010);

        for (int i = 0; i < values.Length; i++)
        {
            Bars.Add(new(Values[i], colors[i]));
        }
        BarWidth = Bars.Count * 32;

    }
}

class BarViewModel : ViewModelBase
{
    private static readonly double Modifier = 32;
    private double height;
    private SolidColorBrush color = new();
    private string barSignAlignment = "Top";
    private string barAlignment = "Top";

    public double Height { get => height; set => this.RaiseAndSetIfChanged(ref height, value); }
    public SolidColorBrush Color { get => color; set => this.RaiseAndSetIfChanged(ref color, value); }
    public string BarSignAlignment { get => barSignAlignment; set => this.RaiseAndSetIfChanged(ref barSignAlignment, value); }
    public string BarAlignment { get => barAlignment; set => this.RaiseAndSetIfChanged(ref barAlignment, value); }

    public BarViewModel(double height, SolidColorBrush color)
    {
        Height = Math.Abs(height) * Modifier;
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

class StringViewModel(string value, SolidColorBrush color) : ViewModelBase
{
    private string text = value;
    public string Text { get => text; set => this.RaiseAndSetIfChanged(ref text, value); }
    private readonly SolidColorBrush color = color;
    public SolidColorBrush Color { get => color; }
}
