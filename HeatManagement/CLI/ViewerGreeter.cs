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
                string json = "INVALID";

                try
                {
                    json = File.ReadAllText(filePath);
                }
                catch
                {
                    return "Data file is inaccessible";
                }

                try
                {
                    sourceData = new(json);
                    if (sourceData.Data!.Count == 0)
                    {
                        sourceData = null;
                        return "Source data file contains no data";
                    }
                }
                catch
                {
                    try
                    {
                        resultData = new(json);
                        if (resultData.Data!.Count == 0)
                        {
                            resultData = null;
                            return "Result data file contains no data";
                        }
                    }
                    catch
                    {
                        return "Data file has invalid json";
                    }
                }
            }

            return "";
        }

        TextBox(
            text: arguments.DataPath ?? "data.json",
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
                    string json = "INVALID";

                    try
                    {
                        json = File.ReadAllText(filePath);
                    }
                    catch
                    {
                        return "Assets file is inaccessible";
                    }
                    try
                    {
                        assets = new(json);
                        if (assets.Assets!.Count == 0)
                        {
                            return "Assets file contains no assets";
                        }
                    }
                    catch
                    {
                        return "Assets file has invalid json";
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