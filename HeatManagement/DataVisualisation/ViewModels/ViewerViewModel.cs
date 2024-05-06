using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text.Json;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Platform.Storage;
using DynamicData;
using HeatManagement.Views;
using ReactiveUI;

namespace HeatManagement.ViewModels;
class ViewerViewModel : ViewModelBase
{
    public ObservableCollection<ViewerButtonViewModel> ViewerButtonValues { get; } = [];
    public ObservableCollection<ViewerButtonViewModel> SelectedGraphs { get; } = [];

    public ObservableCollection<TextBlock> SelectedGraphTitles { get; } = [];
    public ObservableCollection<TextBlock> SelectedGraphValues { get; } = [];
    public ObservableCollection<Polyline> GraphLines { get; } = [];

    private string selectedGraphTime = "";
    public string SelectedGraphTime { get => selectedGraphTime; set => this.RaiseAndSetIfChanged(ref selectedGraphTime, value); }
    private bool graphSelected = false;
    public bool GraphSelected { get => graphSelected; set => this.RaiseAndSetIfChanged(ref graphSelected, value); }
    readonly Dictionary<string, List<double>> GraphList = [];
    readonly Dictionary<string, string> GraphUnits = [];
    readonly List<(DateTime, DateTime)> TimeList = [];
    private bool graphOpen = false;
    public bool GraphOpen { get => graphOpen; set => this.RaiseAndSetIfChanged(ref graphOpen, value); }
    private int graphLength;
    private double graphWidth;
    public double GraphWidth { get => graphWidth; set => this.RaiseAndSetIfChanged(ref graphWidth, value); }
    public ViewerViewModel(ResultDataManager resultDataManager)
    {
        int graphLength = resultDataManager.TimeResults.Count;

        GraphList = new()
        {
            { "Cost",new(new double[graphLength])},
            { "Electricity",new(new double[graphLength])},
            { "CO2",new(new double[graphLength])},
            { "Heat",new(new double[graphLength])}
        };
        GraphUnits = new()
        {
            { "Cost","dkk"},
            { "Electricity","MWh"},
            { "CO2","kg"},
            { "Heat","MWh"}
        };
        ViewerButtonValues.Add(new("Cost", "avares://HeatManagement/DataVisualisation/Assets/Resources/cost.png"));
        ViewerButtonValues.Add(new("Electricity", "avares://HeatManagement/DataVisualisation/Assets/Resources/electricity.png"));
        ViewerButtonValues.Add(new("CO2", "avares://HeatManagement/DataVisualisation/Assets/Resources/co2.png"));
        ViewerButtonValues.Add(new("Heat", "avares://HeatManagement/DataVisualisation/Assets/Resources/heat.png"));

        for (int i = 0; i < graphLength; i++)
        {
            UnitResults time = resultDataManager.TimeResults[i];
            DateTime startTime = time.StartTime;
            DateTime endTime = time.EndTime;
            TimeList.Add((startTime, endTime));
            foreach (Result asset in time.Results)
            {
                string assetName = asset.Asset;
                double producedHeat = asset.ProducedHeat;
                double consumedElectricity = asset.ConsumedElectricity;
                double cost = asset.Cost;
                double producedCO2 = asset.ProducedCO2;

                GraphList["Cost"][i] += cost;
                GraphList["Electricity"][i] += consumedElectricity;
                GraphList["CO2"][i] += producedCO2;
                GraphList["Heat"][i] += producedHeat;

                if (!GraphList.ContainsKey($"{assetName}-Cost"))
                {
                    GraphList[$"{assetName}-Cost"] = new(new double[graphLength]);
                    GraphList[$"{assetName}-Electricity"] = new(new double[graphLength]);
                    GraphList[$"{assetName}-CO2"] = new(new double[graphLength]);
                    GraphList[$"{assetName}-Heat"] = new(new double[graphLength]);
                    GraphUnits[$"{assetName}-Cost"] = "dkk";
                    GraphUnits[$"{assetName}-Electricity"] = "MWh";
                    GraphUnits[$"{assetName}-CO2"] = "kg";
                    GraphUnits[$"{assetName}-Heat"] = "MWh";
                    ViewerButtonValues.Add(new($"{assetName}-Cost", asset.ImagePath));
                    ViewerButtonValues.Add(new($"{assetName}-Electricity", asset.ImagePath));
                    ViewerButtonValues.Add(new($"{assetName}-CO2", asset.ImagePath));
                    ViewerButtonValues.Add(new($"{assetName}-Heat", asset.ImagePath));
                }

                GraphList[$"{assetName}-Cost"][i] = cost;
                GraphList[$"{assetName}-Electricity"][i] = consumedElectricity;
                GraphList[$"{assetName}-CO2"][i] = producedCO2;
                GraphList[$"{assetName}-Heat"][i] = producedHeat;
                foreach ((string resource, double value) in asset.AdditionalResources)
                {
                    if (!GraphList.ContainsKey($"{resource}"))
                    {
                        GraphList[$"{resource}"] = new(new double[graphLength]);
                        GraphUnits[$"{resource}"] = "MWh";
                        ViewerButtonValues.Add(new($"{resource}", $"avares://HeatManagement/DataVisualisation/Assets/Resources/{resource.ToLower()}.png"));
                    }
                    GraphList[$"{resource}"][i] += value;

                    if (!GraphList.ContainsKey($"{assetName}-{resource}"))
                    {
                        GraphList[$"{assetName}-{resource}"] = new(new double[graphLength]);
                        GraphUnits[$"{assetName}-{resource}"] = "MWh";
                        ViewerButtonValues.Add(new($"{assetName}-{resource}", asset.ImagePath));
                    }
                    GraphList[$"{assetName}-{resource}"][i] += value;
                }
            }
        }

        //golden ratio-based color generation
        for (int i = 0; i < ViewerButtonValues.Count; i++) ViewerButtonValues[i].Color = new(HslColor.FromHsl(137.5 * i % 360, 1, 1.618 * i % 0.33 + 0.33).ToRgb());

        SelectedGraphs = [ViewerButtonValues.First()];
    }
    public void CloseGraph()
    {
        GraphOpen = false;
    }

    public void SelectGraphTime(int index)
    {
        index = Math.Clamp(index, 0, graphLength);
        SelectedGraphValues.Clear();
        SelectedGraphValues.AddRange(SelectedGraphs.Select((val) => new TextBlock() { Text = $"{GraphList[val.GraphName][index]:0.##} {GraphUnits[val.GraphName]}", Foreground = val.Color }));

        //set graph line position
        GraphLines[1] = new() { Points = [new(index * 10, 0), new(index * 10, 1000)], Stroke = new SolidColorBrush(Color.FromUInt32(0x80ffffff)), StrokeThickness = 10 };
        SelectedGraphTime = new($"{TimeList[index].Item1} - {TimeList[index].Item2}");
    }

    public async void ExportGraphs()
    {
        List<List<double>> selectedGraphValues = SelectedGraphs.Select((value) => GraphList[value.GraphName]).ToList();
        List<string> selectedGraphNames = SelectedGraphs.Select((value) => value.GraphName).ToList();

        List<string> CSVRows = [];
        CSVRows.Add(string.Join(",", selectedGraphNames));
        for (int i = 0; i < selectedGraphValues.First().Count; ++i)
        {
            List<string> row = [];
            for (int u = 0; u < selectedGraphValues.Count; ++u)
            {
                row.Add(FormattableString.Invariant($"{selectedGraphValues[u][i]:0.##}"));
            }
            CSVRows.Add(string.Join(',', row));
        }

        IStorageFile? file = await App.TopLevel.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            Title = "Export graphs",
            SuggestedFileName = "graphs.csv"
        });

        if (file != null)
        {
            try
            {
                // Open writing stream from the file.
                await using Stream stream = await file.OpenWriteAsync();
                using StreamWriter streamWriter = new(stream);
                await streamWriter.WriteAsync(string.Join("\n", CSVRows));
            }
            catch { }
        }
    }

    public void OpenGraphs()
    {
        var selectedGraphList = SelectedGraphs.Select((val) => (ValueList: GraphList[val.GraphName], val.Color));
        double maxValue = selectedGraphList.Select((val) => val.ValueList.Max()).Max();
        GraphLines.Clear();
        graphLength = selectedGraphList.First().ValueList.Count;
        GraphWidth = graphLength * 10;

        //zero line
        GraphLines.Add(new() { Points = [new(0, 500), new(graphLength * 10, 500)], Stroke = new SolidColorBrush(Colors.White), StrokeThickness = 5, StrokeDashArray = [2, 1] });

        //selection line, also gets edited in SelectGraphTime
        GraphLines.Add(new() { Points = [new(0, 0), new(0, 1000)], Stroke = new SolidColorBrush(Color.FromUInt32(0x80ffffff)), StrokeThickness = 10 });

        int offset = 0;
        foreach (var graphValues in selectedGraphList)
        {
            GraphLines.Add(new() { Stroke = graphValues.Color, StrokeThickness = 3 });
            for (int i = 0; i < graphLength; i++)
            {
                GraphLines.Last().Points.Add(new(i * 10, 500 - graphValues.ValueList[i] / maxValue * 500 + offset));
            }
            offset++;
        }
        SelectedGraphTitles.Clear();
        SelectedGraphTitles.AddRange(SelectedGraphs.Select((val) => new TextBlock() { Text = val.GraphName, Foreground = val.Color }));
        SelectGraphTime(0);

        GraphOpen = true;
    }

    public void SelectGraphPositionPointer(object sender, PointerPressedEventArgs args)
    {
        var point = args.GetCurrentPoint(sender as Control);
        var x = point.Position.X;
        if (point.Properties.IsLeftButtonPressed)
        {
            SelectGraphTime((int)Math.Round(x) / 10);
        }
    }

    public class ViewerButtonViewModel : ViewModelBase
    {
        public string GraphName { get; }
        public Bitmap Image { get; }
        public SolidColorBrush Color { get; set; } = new(Colors.White);


        public ViewerButtonViewModel(string graphName, string imagePath)
        {
            GraphName = graphName;
            Image = new(AssetLoader.Open(new Uri("avares://HeatManagement/DataVisualisation/Assets/Resources/unknown.png")));
            try
            {
                Image = new(AssetLoader.Open(new Uri(imagePath)));
            }
            catch
            {
                try
                {
                    Image = new(imagePath);
                }
                catch { }
            }
        }
    }
}