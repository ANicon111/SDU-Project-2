using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
        void saveAssets()
        {
            string saveFile(string filePath)
            {
                try
                {
                    string extension = filePath.Split('.').Last().ToLower();
                    if (extension == "json")
                    {
                        File.WriteAllText(filePath, assets!.ToJson(jsonOptions));
                    }
                    else
                    {
                        File.WriteAllText(filePath, assets!.ToCSV());
                    }
                }
                catch
                {
                    return "Couldn't save file";
                }
                return "";
            }
            bool escaped = false;
            TextBox(ref escaped, filePath, "Input the output file path", saveFile);
        }

        void saveSourceData()
        {
            string saveFile(string filePath)
            {
                try
                {
                    string extension = filePath.Split('.').Last().ToLower();
                    if (extension == "json")
                    {
                        File.WriteAllText(filePath, sourceData!.ToJson(jsonOptions));
                    }
                    else
                    {
                        File.WriteAllText(filePath, sourceData!.ToCSV());
                    }
                }
                catch
                {
                    return "Couldn't save file";
                }
                return "";
            }
            bool escaped = false;
            TextBox(ref escaped, filePath, "Input the output file path", saveFile);
        }


        //removal functions
        void removeFromAssets(int index) => assets.RemoveAsset(assets!.Assets!.ElementAt(index).Key);
        void removeFromSourceData(int index) => sourceData.RemoveData(sourceData.Data.ElementAt(index).Key.Item1, sourceData.Data.ElementAt(index).Key.Item2);

        //information functions
        string getAsset(int index)
        {
            KeyValuePair<string, Asset> asset = assets!.Assets!.ElementAt(index);
            List<string> additionalResources = [];
            foreach (KeyValuePair<string, AdditionalResource> resource in asset.Value.AdditionalResources)
            {
                additionalResources.Add(FormattableString.Invariant($"{resource.Key}: {resource.Value.Value}, {resource.Value.Measurement}"));
            }
            return FormattableString.Invariant($"""
             Name: {asset.Key} 
             Image path: {asset.Value.ImagePath} 
             Heat capacity: {asset.Value.HeatCapacity} MWh 
             Cost per MWh: {asset.Value.Cost} dkk 
             Electricity production/usage: {asset.Value.ElectricityCapacity} MWh  
             CO2 emissions: {asset.Value.CO2} kg per MWh of heat 
             Additional resources - {(additionalResources.Count != 0 ? string.Join("; ", additionalResources) : "nothing")} 
            """);
        }
        string getSourceData(int index)
        {
            SourceData source = sourceData!.Data.ElementAt(index).Value;
            return FormattableString.Invariant($"""
             Start time: {source.StartTime:yyyy'-'MM'-'dd' 'HH':'mm':'ss} 
             End time: {source.EndTime:yyyy'-'MM'-'dd' 'HH':'mm':'ss} 
             Heat demand: {source.HeatDemand} MWh 
             Electricity price: {source.ElectricityPrice} dkk per MWh 
            """);
        }

        //addition functions
        string? addToAssets()
        {
            string assetName = "";
            string setAssetName(string text)
            {
                if (assets.Assets.ContainsKey(text)) return "Asset name already exists";
                assetName = text.Trim().ToUpper();
                return "";
            };
            bool escaped = false;
            TextBox(ref escaped, "", "Enter the asset name", setAssetName);

            string imagePath = "";
            string setImagePath(string text) { imagePath = text.Trim(); return ""; };
            TextBox(ref escaped, "", "Enter the asset image path", setImagePath);

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
            TextBox(ref escaped, "", "Enter the heat production capacity in MWh", parseHeatCapacity, numbersOnly: true);

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
            TextBox(ref escaped, "", "Enter the cost in dkk per MWh of heat", parseCost, numbersOnly: true);

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
            TextBox(ref escaped, "", "Enter the electricity production/usage in MWh (usage is negative)", parseElectricity, numbersOnly: true);

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
            TextBox(ref escaped, "", "Enter the CO2 emissions in kg", parseCO2, numbersOnly: true);

            Dictionary<string, AdditionalResource> additionalResources = [];
            string parseAdditionalResources(string text)
            {
                if (text.Trim() != "")
                    try
                    {
                        string[] resources = text.Split([";"], StringSplitOptions.None);
                        foreach (string resource in resources)
                        {
                            string[] resourceNameValueMeasurement = resource.Split([":", ","], StringSplitOptions.None);
                            additionalResources[resourceNameValueMeasurement[0].Trim()] =
                                new(Convert.ToDouble(resourceNameValueMeasurement[1], CultureInfo.InvariantCulture), resourceNameValueMeasurement[2].Trim());
                        }
                    }
                    catch
                    {
                        additionalResources = [];
                        return "Couldn't parse additional resources";
                    }
                return "";
            }
            TextBox(ref escaped, "", "Enter additional resource consumption or production (oil: -1.2, MWh; water: 52.4, kg)", parseAdditionalResources);

            if (!escaped)
            {
                assets.AddAsset(assetName, new(imagePath, heatCapacity, cost, electricityCapacity, co2, additionalResources));
                return assetName;
            }
            else
                return null;
        }

        string? addToSourceData()
        {
            bool escaped = false;
            DateTime dataStartTime = new();
            string parseStartTime(string text)
            {
                try
                {
                    dataStartTime = DateTime.ParseExact(text, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
                }
                catch
                {
                    try
                    {
                        dataStartTime = DateTime.ParseExact(text, "yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture);
                    }
                    catch
                    {
                        return "Couldn't parse time";
                    }
                }
                return "";
            }
            TextBox(ref escaped, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture), "Enter the start time (yyyy-MM-dd hh:mm[:ss])", parseStartTime, numbersOnly: true);

            DateTime dataEndTime = dataStartTime.AddHours(1);
            string parseEndTime(string text)
            {
                try
                {
                    dataEndTime = DateTime.ParseExact(text, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
                }
                catch
                {
                    try
                    {
                        dataEndTime = DateTime.ParseExact(text, "yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture);
                    }
                    catch
                    {
                        return "Couldn't parse time";
                    }
                }
                return "";
            }
            TextBox(ref escaped, dataEndTime.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture), "Enter the end time (yyyy-MM-dd hh:mm[:ss])", parseEndTime, numbersOnly: true);

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
            TextBox(ref escaped, "", "Enter the heat demand in MWh", parseHeatDemand, numbersOnly: true);

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
            TextBox(ref escaped, "", "Enter the electricity price in dkk per MWh", parseElectricityPrice, numbersOnly: true);

            if (!escaped)
            {
                sourceData.AddData(new(dataStartTime, dataEndTime, heatDemand, electricityPrice));
                return $"{dataStartTime:yyyy'-'MM'-'dd' 'HH':'mm':'ss} - {dataEndTime:yyyy'-'MM'-'dd' 'HH':'mm':'ss}";
            }
            else
            {
                return null;
            }
        }
        List<string> importToSourceData()
        {
            bool escaped = false;
            DateTime dataStartTime = new();
            string parseStartTime(string text)
            {
                try
                {
                    dataStartTime = DateTime.ParseExact(text, "yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture);
                }
                catch
                {
                    return "Couldn't parse time";
                }
                return "";
            }
            TextBox(ref escaped, DateTime.Now.ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture), "Enter the import start time (yyyy-MM-dd hh:mm)", parseStartTime, numbersOnly: true);

            DateTime dataEndTime = dataStartTime.AddDays(7);
            string parseEndTime(string text)
            {
                try
                {
                    dataEndTime = DateTime.ParseExact(text, "yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture);
                }
                catch
                {
                    return "Couldn't parse time";
                }
                return "";
            }
            TextBox(ref escaped, dataEndTime.ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture), "Enter the import end time (yyyy-MM-dd hh:mm)", parseEndTime, numbersOnly: true);

            List<SourceData> data = [];
            List<string> names = [];

            sourceData = Utils.GetSourceDataFromAPI(dataStartTime, dataEndTime).GetAwaiter().GetResult();
            foreach (var source in sourceData.Data)
            {
                names.Add($"{source.Value.StartTime:yyyy'-'MM'-'dd' 'HH':'mm':'ss} - {source.Value.EndTime:yyyy'-'MM'-'dd' 'HH':'mm':'ss}");
            }

            return names;
        }

        List<string> names;
        switch (fileType)
        {
            case "asset":
                names = new(assets!.Assets!.Count);
                foreach (KeyValuePair<string, Asset> asset in assets.Assets) names.Add(asset.Key);
                EntryList(true, names, addToAssets, null, getAsset, removeFromAssets, saveAssets);
                break;
            case "sourceData":
                names = new(sourceData!.Data.Count);
                foreach (KeyValuePair<Tuple<DateTime, DateTime>, SourceData> element in sourceData.Data) names.Add($"{element.Value.StartTime:yyyy'-'MM'-'dd' 'HH':'mm':'ss} - {element.Value.EndTime:yyyy'-'MM'-'dd' 'HH':'mm':'ss}");
                EntryList(false, names, addToSourceData, importToSourceData, getSourceData, removeFromSourceData, saveSourceData);
                break;
        }
    }

    //shows all names, allows addition and deletion
    static void EntryList(bool assetManager, List<string> names, Func<string?> addToManager, Func<List<string>>? importToSourceDataManager, Func<int, string> getData, Action<int> removeFromManager, Action save)
    {
        int selectedValue = 0;
        int menuHeight() => names.Count + 9;
        int menuWidth()
        {
            int width = 53;
            foreach (string name in names)
            {
                if (width < name.Length + 4) width = name.Length + 4;
            }
            return width;
        };
        int titleLength() => assetManager ? 8 : 9;
        string title() => assetManager ?
                """
                 ↑ ↓   change the selected value
                  A    add a value to the manager
                ENTER  display the selection values
                 DEL   delete the selected value
                  S    save manager to file
                 ESC   quit the application
                """
                :
                """
                 ↑ ↓   change the selected value
                  A    add a value to the manager
                ENTER  display the selection values
                 DEL   delete the selected value
                  S    save manager to file
                 ESC   quit the application
                  I    import source data from energidataservice.dk
                """;

        int menuPosition() => -Math.Clamp(selectedValue - renderer.TerminalHeight / 2 + titleLength(), 0, Math.Max(names.Count + titleLength() + 1 - renderer.TerminalHeight, 0));
        int selectorPosition() => selectedValue + menuPosition() + titleLength();

        renderer.Object = new(
            geometry: new(0, 0, renderer.TerminalWidth, renderer.TerminalHeight),
            subObjects: [
                new(
                    geometry: new(0, menuPosition(), menuWidth(), menuHeight()),
                    internalAlignmentX: Alignment.Center,
                    externalAlignmentX: Alignment.Center,
                    border: Borders.Rounded,
                    defaultCharacter:' '
                ),
                new(
                    geometry: new(0, selectorPosition(), menuWidth()-2, 1),
                    internalAlignmentX: Alignment.Center,
                    externalAlignmentX: Alignment.Center,
                    defaultCharacter:' ',
                    colorAreas: [
                        new(Colors.White.WithAlpha(0.5))
                    ]
                )
            ]
        );

        RendererObject list() => renderer.Object.SubObjects[0];
        RendererObject selector() => renderer.Object.SubObjects[1];

        //color getters for readability
        ObservableCollection<ColorArea> selectedElementColor() => [new(Colors.White.WithAlpha(0.25))];
        ObservableCollection<ColorArea> unselectedElementColor() => [];
        //create a RendererObject for each name
        void fillNameList()
        {
            list().Height = menuHeight();
            list().Width = menuWidth();
            list().SubObjects.Add(new(
                text: title(),
                externalAlignmentX: Alignment.Center,
                y: 1
            ));
            for (int i = 0; i < names.Count; i++)
            {
                list().SubObjects.Add(new(
                    text: names[i],
                    y: i + titleLength()
                ));
            }
            list().Update();
        }
        fillNameList();
        if (names.Count > 0)
        {
            selector().ColorAreas = selectedElementColor();
            selector().Y = selectorPosition();
        }
        else
        {
            selector().ColorAreas = unselectedElementColor();
        }


        renderer.Update(forceRedraw: true);
        bool running = true;


        while (running)
        {
            while (Console.KeyAvailable)
            {
                ConsoleKeyInfo consoleKeyInfo = renderer.ReadKey();
                int moveSpace = consoleKeyInfo.Modifiers == ConsoleModifiers.Control ? 5 : 1;
                switch (consoleKeyInfo.Key)
                {
                    //switch selected value and move the menu to center the selection
                    case ConsoleKey.UpArrow:
                        if (names.Count > 0)
                        {
                            selectedValue = Math.Max(selectedValue - moveSpace, 0);
                            renderer.Object.SubObjects[0].Y = menuPosition();
                            selector().Y = selectorPosition();
                        }
                        break;
                    case ConsoleKey.DownArrow:
                        if (names.Count > 0)
                        {
                            selectedValue = Math.Min(selectedValue + moveSpace, names.Count - 1);
                            renderer.Object.SubObjects[0].Y = menuPosition();
                            selector().Y = selectorPosition();
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
                            {
                                selector().ColorAreas = selectedElementColor();
                                selector().Y = selectorPosition();
                            }
                            else
                            {
                                selector().ColorAreas = unselectedElementColor();
                            }
                        }
                        break;

                    case ConsoleKey.A:
                        string? name = addToManager();
                        if (name != null)
                        {
                            names.Add(name);
                            names.Sort();
                            selectedValue = names.IndexOf(name);
                            list().SubObjects.Clear();
                            fillNameList();
                            selector().ColorAreas = selectedElementColor();
                            selector().Y = selectorPosition();
                        }
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
                                            {
                                                selector().ColorAreas = selectedElementColor();
                                                selector().Y = selectorPosition();
                                            }
                                            else
                                            {
                                                selector().ColorAreas = unselectedElementColor();
                                            }
                                            displaying = false;
                                            break;
                                        case ConsoleKey.Enter:
                                            displaying = false;
                                            break;
                                        case ConsoleKey.Escape:
                                            displaying = false;
                                            running = false;
                                            break;
                                    }
                                }
                                Thread.Sleep(25);
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
                    case ConsoleKey.Escape:
                        running = false;
                        break;

                    //import source data
                    case ConsoleKey.I:
                        if (importToSourceDataManager != null)
                        {
                            names = importToSourceDataManager();
                            names.Sort();
                            selectedValue = 0;
                            list().SubObjects.Clear();
                            fillNameList();
                            if (names.Count > 0)
                            {
                                selector().ColorAreas = selectedElementColor();
                                selector().Y = selectorPosition();
                            }
                            else
                            {
                                selector().ColorAreas = unselectedElementColor();
                            }
                        }
                        break;
                }
            }

            Thread.Sleep(25);
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