using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using AnsiRenderer;
namespace HeatManagement.CLI;

static partial class App
{

    static void RunEditor(string fileType, string filePath, AssetManager? assets, SourceDataManager? sourceData)
    {
        //initialize edited manager
        switch (fileType)
        {
            case "asset": assets ??= new(); break;
            case "sourceData": sourceData ??= new(); break;
        }

        //save manager data
        JsonSerializerOptions jsonOptions = new() { WriteIndented = true };
        void saveAssets() => File.WriteAllText(filePath, assets!.ToJson(jsonOptions));
        void saveSourceData() => File.WriteAllText(filePath, sourceData!.ToJson(jsonOptions));


        //removal functions
        void removeFromAssets(int index) => assets.RemoveAsset(assets!.Assets!.ElementAt(index).Key);
        void removeFromSourceData(int index) => sourceData.RemoveData(sourceData.Data.ElementAt(index).Key.Item1, sourceData.Data.ElementAt(index).Key.Item2);

        //information functions
        string getAsset(int index)
        {
            KeyValuePair<string, Asset> asset = assets!.Assets!.ElementAt(index);
            List<string> additionalResources = [];
            foreach (KeyValuePair<string, double> resource in asset.Value.AdditionalResources)
            {
                additionalResources.Add(FormattableString.Invariant($"{resource.Key}: {resource.Value}"));
            }
            return FormattableString.Invariant($"""
             Name: {asset.Key} 
             Image path: {asset.Value.ImagePath} 
             Heat capacity: {asset.Value.HeatCapacity} MWh 
             Cost per MWh: {asset.Value.Cost} dkk 
             Electricity production/usage: {asset.Value.ElectricityCapacity} MWh  
             CO2 emissions: {asset.Value.CO2} kg per MWh of heat 
             Additional resources - {(additionalResources.Count != 0 ? string.Join(", ", additionalResources) : "nothing")} 
            """);
        }
        string getSourceData(int index)
        {
            var source = sourceData!.Data.ElementAt(index).Value;
            return FormattableString.Invariant($"""
             Start time: {source.StartTime:dd'.'MM'.'yyyy' 'HH':'mm':'ss} 
             End time: {source.EndTime:dd'.'MM'.'yyyy' 'HH':'mm':'ss} 
             Heat demand: {source.HeatDemand} MWh 
             Electricity price: {source.ElectricityPrice} dkk per MWh 
            """);
        }

        //addition functions
        string addToAssets()
        {
            string assetName = "";
            string setAssetName(string text)
            {
                if (assets.Assets.ContainsKey(text)) return "Asset name already exists";
                assetName = text.Trim().ToUpper();
                return "";
            };
            TextBox("", "Enter the asset name", setAssetName);

            string imagePath = "";
            string setImagePath(string text) { imagePath = text.Trim(); return ""; };
            TextBox("", "Enter the asset image path", setImagePath);

            double heatCapacity = 0;
            string parseHeatCapacity(string text)
            {
                try
                {
                    heatCapacity = Convert.ToDouble(text, CultureInfo.InvariantCulture);
                }
                catch
                {

                    return "Couldn't parse heat production";
                }
                return "";
            }
            TextBox("", "Enter the heat production capacity in MWh", parseHeatCapacity, numbersOnly: true);

            double cost = 0;
            string parseCost(string text)
            {
                try
                {
                    cost = Convert.ToDouble(text, CultureInfo.InvariantCulture);
                }
                catch
                {

                    return "Couldn't parse cost";
                }
                return "";
            }
            TextBox("", "Enter the cost in dkk per MWh of heat", parseCost, numbersOnly: true);

            double electricityCapacity = 0;
            string parseElectricity(string text)
            {
                try
                {
                    electricityCapacity = Convert.ToDouble(text, CultureInfo.InvariantCulture);
                }
                catch
                {

                    return "Couldn't parse electricity capacity";
                }
                return "";
            }
            TextBox("", "Enter the electricity production/usage in MWh (usage is negative)", parseElectricity, numbersOnly: true);

            double co2 = 0;
            string parseCO2(string text)
            {
                try
                {
                    co2 = Convert.ToDouble(text, CultureInfo.InvariantCulture);
                }
                catch
                {

                    return "Couldn't parse CO2 emissions";
                }
                return "";
            }
            TextBox("", "Enter the CO2 emissions in kg", parseCO2, numbersOnly: true);

            Dictionary<string, double> additionalResources = [];
            string parseAdditionalResources(string text)
            {
                if (text.Trim() != "")
                    try
                    {
                        string[] resources = text.Split(',');
                        foreach (string resource in resources)
                        {
                            string[] resourceNameAndValue = resource.Split(':');
                            additionalResources[resourceNameAndValue[0].Trim()] = Convert.ToDouble(resourceNameAndValue[1], CultureInfo.InvariantCulture);
                        }
                    }
                    catch
                    {
                        additionalResources = [];
                        return "Couldn't parse additional resources";
                    }
                return "";
            }
            TextBox("", "Enter additional resources (gas: 1.2, oil: 0.2)", parseAdditionalResources);

            assets.AddAsset(assetName, new(imagePath, heatCapacity, cost, electricityCapacity, co2, additionalResources));
            return assetName;
        }

        string addToSourceData()
        {
            DateTime dataStartTime = new();
            string parseStartTime(string text)
            {
                try
                {
                    dataStartTime = DateTime.ParseExact(text, "dd.MM.yyyy HH:mm:ss", CultureInfo.InvariantCulture);
                }
                catch
                {
                    try
                    {
                        dataStartTime = DateTime.ParseExact(text, "dd.MM.yyyy HH:mm", CultureInfo.InvariantCulture);
                    }
                    catch
                    {
                        return "Couldn't parse date and time";
                    }
                }
                return "";
            }
            TextBox(DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss", CultureInfo.InvariantCulture), "Enter the start date and time (dd.mm.yyyy hh:mm[:ss])", parseStartTime, numbersOnly: true);

            DateTime dataEndTime = dataStartTime.AddHours(1);
            string parseEndTime(string text)
            {
                try
                {
                    dataEndTime = DateTime.ParseExact(text, "dd.MM.yyyy HH:mm:ss", CultureInfo.InvariantCulture);
                }
                catch
                {
                    try
                    {
                        dataEndTime = DateTime.ParseExact(text, "dd.MM.yyyy HH:mm", CultureInfo.InvariantCulture);
                    }
                    catch
                    {
                        return "Couldn't parse date and time";
                    }
                }
                return "";
            }
            TextBox(dataEndTime.ToString("dd.MM.yyyy HH:mm:ss", CultureInfo.InvariantCulture), "Enter the end date and time (dd.mm.yyyy hh:mm[:ss])", parseEndTime, numbersOnly: true);

            double heatDemand = 0;
            string parseHeatDemand(string text)
            {
                try
                {
                    heatDemand = Convert.ToDouble(text, CultureInfo.InvariantCulture);
                }
                catch
                {

                    return "Couldn't parse heat demand";
                }
                return "";
            }
            TextBox("", "Enter the heat demand in MWh", parseHeatDemand, numbersOnly: true);

            double electricityPrice = 0;
            string parseElectricityPrice(string text)
            {
                try
                {
                    electricityPrice = Convert.ToDouble(text, CultureInfo.InvariantCulture);
                }
                catch
                {

                    return "Couldn't parse electricity price";
                }
                return "";
            }
            TextBox("", "Enter the electricity price in dkk per MWh", parseElectricityPrice, numbersOnly: true);

            sourceData.AddData(new(dataStartTime, dataEndTime, heatDemand, electricityPrice));
            return $"{dataStartTime:dd'.'MM'.'yyyy' 'HH':'mm':'ss} - {dataEndTime:dd'.'MM'.'yyyy' 'HH':'mm':'ss}";
        }

        List<string> names;
        switch (fileType)
        {
            case "asset":
                names = new(assets!.Assets!.Count);
                foreach (var asset in assets.Assets) names.Add(asset.Key);
                EntryList(names, addToAssets, getAsset, removeFromAssets, saveAssets);
                break;
            case "sourceData":
                names = new(sourceData!.Data.Count);
                foreach (var element in sourceData.Data) names.Add($"{element.Value.StartTime:dd'.'MM'.'yyyy' 'HH':'mm':'ss} - {element.Value.EndTime:dd'.'MM'.'yyyy' 'HH':'mm':'ss}");
                EntryList(names, addToSourceData, getSourceData, removeFromSourceData, saveSourceData);
                break;
        }
    }

    //shows all names, allows addition and deletion
    static void EntryList(List<string> names, Func<string> addToManager, Func<int, string> getData, Action<int> removeFromManager, Action save)
    {
        int selectedValue = 0;
        int menuHeight() => names.Count + 9;
        int menuWidth()
        {
            int width = 39;
            foreach (var name in names)
            {
                if (width < name.Length + 4) width = name.Length + 4;
            }
            return width;
        };
        int menuPosition() => -Math.Clamp(selectedValue - renderer.TerminalHeight / 2 + 8, 0, Math.Max(names.Count + 9 - renderer.TerminalHeight, 0));

        renderer.Object = new(
            geometry: new(0, 0, renderer.TerminalWidth, renderer.TerminalHeight),
            subObjects: [
                new(
                    geometry: new(0, menuPosition(), menuWidth(), menuHeight()),
                    internalAlignmentX: Alignment.Center,
                    externalAlignmentX: Alignment.Center,
                    border: Borders.Rounded,
                    defaultCharacter:' '
                )
            ]
        );

        RendererObject list() => renderer.Object.SubObjects[0];
        //create a RendererObject for each name
        void fillNameList()
        {
            list().Height = menuHeight();
            list().Width = menuWidth();
            list().SubObjects.Add(new(
                text:
                """
                 ↑ ↓   change the selected value
                  A    add a value to the manager
                ENTER  display the selection values
                 DEL   delete the selected value
                  S    save manager to file
                  Q    quit the application
                """,
                externalAlignmentX: Alignment.Center,
                y: 1
            ));
            for (int i = 0; i < names.Count; i++)
            {
                list().SubObjects.Add(new(
                    text: names[i],
                    y: i + 8
                ));
            }
            list().Update();
        }
        fillNameList();
        if (names.Count > 0)
            list().SubObjects[selectedValue + 1].ColorAreas = selectedElementColor();

        renderer.Update(forceRedraw: true);
        bool running = true;

        //color getters for readability
        List<ColorArea> selectedElementColor() => [new(Colors.White), new(Colors.Black, true)];
        List<ColorArea> unselectedElementColor() => [];

        while (running)
        {
            while (Console.KeyAvailable)
            {
                switch (renderer.ReadKey().Key)
                {
                    //switch selected value and move the menu to center the selection
                    case ConsoleKey.UpArrow:
                        if (names.Count > 0)
                        {
                            list().SubObjects[selectedValue + 1].ColorAreas = unselectedElementColor();
                            selectedValue = Math.Max(selectedValue - 1, 0);
                            renderer.Object.SubObjects[0].Y = menuPosition();
                            list().SubObjects[selectedValue + 1].ColorAreas = selectedElementColor();
                        }
                        break;
                    case ConsoleKey.DownArrow:
                        if (names.Count > 0)
                        {
                            list().SubObjects[selectedValue + 1].ColorAreas = unselectedElementColor();
                            selectedValue = Math.Min(selectedValue + 1, names.Count - 1);
                            renderer.Object.SubObjects[0].Y = menuPosition();
                            list().SubObjects[selectedValue + 1].ColorAreas = selectedElementColor();
                        }
                        break;

                    case ConsoleKey.Delete:
                        if (names.Count > 0)
                        {
                            names.RemoveAt(selectedValue);
                            list().SubObjects.RemoveAt(selectedValue + 1);
                            removeFromManager(selectedValue);
                            list().SubObjects.Clear();
                            fillNameList();
                            selectedValue = Math.Min(selectedValue, names.Count - 1);
                            if (names.Count > 0)
                                list().SubObjects[selectedValue + 1].ColorAreas = selectedElementColor();
                        }
                        break;

                    case ConsoleKey.A:
                        string name = addToManager();
                        names.Add(name);
                        names.Sort();
                        selectedValue = names.IndexOf(name);
                        list().SubObjects.Clear();
                        fillNameList();
                        list().SubObjects[selectedValue + 1].ColorAreas = selectedElementColor();
                        break;

                    case ConsoleKey.S:
                        save();
                        break;

                    //display selection info
                    case ConsoleKey.Enter:
                        if (names.Count > 0)
                        {

                            renderer.Object.SubObjects.Add(new());
                            bool displaying = true;
                            while (displaying)
                            {
                                renderer.Object.SubObjects[^1] =
                                        new(
                                            externalAlignmentX: Alignment.Center,
                                            externalAlignmentY: Alignment.Center,
                                            defaultCharacter: ' ',
                                            text: getData(selectedValue),
                                            colorAreas: [
                                                new(color:Colors.Black.WithAlpha(0.75)),
                                            ],
                                            border: Borders.Rounded
                                        );
                                while (Console.KeyAvailable)
                                {
                                    ConsoleKeyInfo key = renderer.ReadKey();
                                    switch (key.Key)
                                    {
                                        case ConsoleKey.Delete:
                                            names.RemoveAt(selectedValue);
                                            list().SubObjects.RemoveAt(selectedValue + 1);
                                            removeFromManager(selectedValue);
                                            list().SubObjects.Clear();
                                            fillNameList();
                                            selectedValue = Math.Min(selectedValue, names.Count - 1);
                                            if (names.Count > 0)
                                                list().SubObjects[selectedValue + 1].ColorAreas = selectedElementColor();
                                            displaying = false;
                                            break;
                                        case ConsoleKey.Enter:
                                            displaying = false;
                                            break;
                                        case ConsoleKey.Q:
                                            displaying = false;
                                            running = false;
                                            break;
                                    }
                                }
                                Thread.Sleep(50);
                                if (renderer.UpdateScreenSize())
                                {
                                    renderer.Object.Width = renderer.TerminalWidth;
                                    renderer.Object.Height = renderer.TerminalHeight;
                                    renderer.Object.SubObjects[0].Y = menuPosition();
                                }
                                renderer.Update();
                            }
                            renderer.Object.SubObjects.RemoveAt(renderer.Object.SubObjects.Count - 1);
                        }
                        break;

                    //quit
                    case ConsoleKey.Q:
                        running = false;
                        break;
                }
            }

            Thread.Sleep(50);
            if (renderer.UpdateScreenSize())
            {
                renderer.Object.Width = renderer.TerminalWidth;
                renderer.Object.Height = renderer.TerminalHeight;
                renderer.Object.SubObjects[0].Y = menuPosition();
            }
            renderer.Update();
        }
        renderer.Reset();
        Console.Clear();
        Console.SetCursorPosition(0, 0);
    }
}