using System;
using System.IO;
using AnsiRenderer;
namespace HeatManagement.CLI;

static partial class App
{
    static void RunViewerGreeter(Arguments arguments)
    {
        renderer.Object = new(
            geometry: new(0, 0, renderer.TerminalWidth, renderer.TerminalHeight),
            internalAlignmentX: Alignment.Center,
            internalAlignmentY: Alignment.Center,
            defaultCharacter: ' '
        );

        SourceDataManager? sourceData = null;
        ResultDataManager? resultData = null;

        string tryLoadSourceResultDataFile(string filePath)
        {
            if (!File.Exists(filePath))
                return "Data file path is invalid";
            else
            {
                string text = "INVALID";

                try
                {
                    text = File.ReadAllText(filePath);
                }
                catch
                {
                    return "Data file is inaccessible";
                }

                try
                {
                    sourceData = SourceDataManager.FromAnySupportedFormat(text);
                    if (sourceData.Data.Count == 0)
                    {
                        sourceData = null;
                        return "Source data file contains no data";
                    }
                }
                catch
                {
                    try
                    {
                        resultData = new(text);
                        if (resultData.Data.Count == 0)
                        {
                            resultData = null;
                            return "Result data file contains no data";
                        }
                    }
                    catch
                    {
                        return "Data file has invalid data";
                    }
                }
            }

            return "";
        }

        TextBox(
            text: arguments.DataPath ?? "data.csv",
            title: "Input the source or result data file path:",
            fileAction: tryLoadSourceResultDataFile,
            tryInitialAction: arguments.DataPath != null
        );

        AssetManager? assets = null;
        if (sourceData != null)
        {
            string tryLoadAssetsFile(string filePath)
            {
                if (!File.Exists(filePath))
                    return "Assets file path is invalid";
                else
                {
                    string text = "INVALID";

                    try
                    {
                        text = File.ReadAllText(filePath);
                    }
                    catch
                    {
                        return "Assets file is inaccessible";
                    }
                    try
                    {
                        assets = AssetManager.FromAnySupportedFormat(text);
                        if (assets.Assets!.Count == 0)
                        {
                            return "Assets file contains no assets";
                        }
                    }
                    catch (Exception assetError)
                    {
                        if (assetError.Message != "Invalid Data")
                            return assetError.Message;
                        return "Assets file has invalid data";
                    }
                }

                return "";
            }

            TextBox(
                text: arguments.AssetsPath ?? "assets.json",
                title: "Input the asset file path:",
                fileAction: tryLoadAssetsFile,
                tryInitialAction: arguments.AssetsPath != null
            );
        }

        RunGraphList(assets, sourceData, resultData);
    }
}