using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;

namespace HeatManagement;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        AssetManager assetManager = new(generateFileIfNotExists: true);
        SourceDataManager sourceDataManager = new(generateFileIfNotExists: true);
        ResultDataManager resultDataManager = new(overwriteFile: true);
        Optimizer.GetResult(assetManager, sourceDataManager, resultDataManager);
        InitializeComponent();
        canvas.Width = ClientSize.Width;
        canvas.Height = ClientSize.Height;
        int i = 0;
        int resultCount = resultDataManager.Data!.Count;
        double peakCost = 0;
        double lowestCost = 1 / 0.0;
        double peakElectricity = 0;
        double lowestElectricity = 1 / 0.0;
        double peakCO2 = 0;
        double lowestCO2 = 1 / 0.0;
        foreach (ResultData result in resultDataManager.Data!)
        {
            if (result.Cost > peakCost) peakCost = result.Cost;
            if (result.Cost < lowestCost) lowestCost = result.Cost;
            if (result.ElectricityUsage > peakElectricity) peakElectricity = result.ElectricityUsage;
            if (result.ElectricityUsage < lowestElectricity) lowestElectricity = result.ElectricityUsage;
            if (result.CO2 > peakCO2) peakCO2 = result.CO2;
            if (result.CO2 < lowestCO2) lowestCO2 = result.CO2;
        }
        foreach (ResultData result in resultDataManager.Data!)
        {
            cost.Points.Add(new(canvas.Width / resultCount * i, canvas.Height / 2 * (1 - result.Cost / (peakCost - Math.Min(0, lowestCost)))));
            electricity.Points.Add(new(canvas.Width / 2 / resultCount * i, canvas.Height / 2 * (2 + (Math.Min(0, lowestElectricity) - result.ElectricityUsage) / (peakElectricity - Math.Min(0, lowestElectricity)))));
            co2.Points.Add(new(canvas.Width / 2 + canvas.Width / 2 / resultCount * i, canvas.Height / 2 * (2 - result.CO2 / peakCO2)));
            i++;
        }
        cost_bg.Width = canvas.Width;
        cost_bg.Height = canvas.Height / 2;
        electricity_bg.Width = canvas.Width / 2;
        electricity_bg.Height = canvas.Height / 2;
        co2_bg.Width = canvas.Width / 2;
        co2_bg.Height = canvas.Height / 2;
        cost_text.Text = $"Cost (highest: {peakCost:0.00} dkk; lowest: {lowestCost:0.00} dkk)";
        electricity_text.Text = $"Electricity (highest production: {peakElectricity:0.00} MWH; highest consumption: {-lowestElectricity:0.00} MWH)";
        co2_text.Text = $"CO2 Emissions (highest: {peakCO2:0.00} kg; lowest: {lowestCO2:0.00} kg)";
        cost_0.StartPoint = new(0, canvas.Height / 2 * (1 + Math.Min(0, lowestCost) / (peakCost - Math.Min(0, lowestCost))));
        cost_0.EndPoint = new(canvas.Width, canvas.Height / 2 * (1 + Math.Min(0, lowestCost) / (peakCost - Math.Min(0, lowestCost))));
        electricity_0.StartPoint = new(0, canvas.Height / 2 * (2 + Math.Min(0, lowestElectricity) / (peakElectricity - Math.Min(0, lowestElectricity))));
        electricity_0.EndPoint = new(canvas.Width / 2, canvas.Height / 2 * (2 + Math.Min(0, lowestElectricity) / (peakElectricity - Math.Min(0, lowestElectricity))));
    }
}