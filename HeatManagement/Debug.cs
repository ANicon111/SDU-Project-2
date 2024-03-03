namespace HeatManagement;
using System.Collections.Generic;


class Debug
{
    public static string UnitTestTest()
    {
        return "test";
    }

    public static void AtCompileRun()
    {
        Dictionary<string, AssetManager> scenarioList = new()
        {
            {"scenario1",new AssetManager("Data/assetsScenario1.json")},
            {"scenario2",new AssetManager("Data/assetsScenario2.json")}
        };
        Dictionary<string, SourceDataManager> dataList = new()
        {
            {"winter",new SourceDataManager("Data/winterData.json")},
            {"summer",new SourceDataManager("Data/summerData.json")}
        };
        Dictionary<string, ResultDataManager> results = [];
        foreach ((string scenarioName, AssetManager assetManager) in scenarioList)
        {
            foreach ((string dataName, SourceDataManager sourceDataManager) in dataList)
            {
                ResultDataManager resultDataManager = new($"Data/{scenarioName}-{dataName}.json", overwriteFile: true);
                Optimizer.GetResult(assetManager, sourceDataManager, resultDataManager);
                results.Add($"{scenarioName}-{dataName}", resultDataManager);
            }
        }
    }
}