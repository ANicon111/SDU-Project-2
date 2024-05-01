namespace HeatManagement.tests;

public class ResultDataManagerTests
{
    [Fact]
    public void JsonTest()
    {
        string expected = """
        [
            {
                "StartTime": "2023-02-08T02:00:00",
                "EndTime": "2023-02-08T03:00:00",
                "Results": [
                    {
                        "Asset": "GB",
                        "ProducedHeat": 2,
                        "ConsumedElectricity": 0,
                        "Cost": 1200,
                        "ProducedCO2": 800,
                        "AdditionalResources": {
                            "gas": 3.8
                        }
                    }
                ]
            },
            {
                "StartTime": "2023-02-08T03:00:00",
                "EndTime": "2023-02-08T04:00:00",
                "Results": [
                    {
                        "Asset": "OB",
                        "ProducedHeat": 2,
                        "ConsumedElectricity": 0,
                        "Cost": 1200,
                        "ProducedCO2": 800,
                        "AdditionalResources": {
                            "oil": 3.8
                        }
                    }
                ]
            }
        ]
        """.Replace(" ", "").Replace("\n", "").Replace("\r", "");
        ResultDataManager resultDataManager = new();
        resultDataManager.JsonImport(expected);
        string actual = resultDataManager.JsonExport();
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void AdditionTest()
    {
        UnitResults unitResults = new()
        {
            StartTime = new DateTime(year: 2023, month: 02, day: 08, hour: 02, minute: 0, second: 0),
            EndTime = new DateTime(year: 2023, month: 02, day: 08, hour: 03, minute: 0, second: 0),
            Results = []
        };
        unitResults.Results.Add(
            new(
                asset: "GB",
                producedHeat: 2,
                consumedElectricity: 0,
                cost: 1200,
                producedCO2: 800,
                additionalResources: new(){
                    {"gas", 3.8}
                }
            )
        );

        ResultDataManager resultDataManager = new();
        resultDataManager.DataAdd(unitResults);
        string expected = """
        [
            {
                "StartTime": "2023-02-08T02:00:00",
                "EndTime": "2023-02-08T03:00:00",
                "Results": [
                    {
                        "Asset": "GB",
                        "ProducedHeat": 2,
                        "ConsumedElectricity": 0,
                        "Cost": 1200,
                        "ProducedCO2": 800,
                        "AdditionalResources": {
                            "gas": 3.8
                        }
                    }
                ]
            }
        ]
        """.Replace(" ", "").Replace("\n", "").Replace("\r", "");
        string actual = resultDataManager.JsonExport();
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void RemovalTest()
    {
        ResultDataManager resultDataManager = new();
        resultDataManager.JsonImport(
            """
            [
                {
                    "StartTime": "2023-02-08T02:00:00",
                    "EndTime": "2023-02-08T03:00:00",
                    "Results": [
                        {
                            "Asset": "GB",
                            "ProducedHeat": 2,
                            "ConsumedElectricity": 0,
                            "Cost": 1200,
                            "ProducedCO2": 800,
                            "AdditionalResources": {
                                "gas": 3.8
                            }
                        }
                    ]
                },
                {
                    "StartTime": "2023-02-08T03:00:00",
                    "EndTime": "2023-02-08T04:00:00",
                    "Results": [
                        {
                            "Asset": "OB",
                            "ProducedHeat": 2,
                            "ConsumedElectricity": 0,
                            "Cost": 1200,
                            "ProducedCO2": 800,
                            "AdditionalResources": {
                                "oil": 3.8
                            }
                        }
                    ]
                }
            ]
            """
        );
        resultDataManager.DataRemove(new(year: 2023, month: 02, day: 08, hour: 03, minute: 0, second: 0));
        string expected = """
        [
            {
                "StartTime": "2023-02-08T02:00:00",
                "EndTime": "2023-02-08T03:00:00",
                "Results": [
                    {
                        "Asset": "GB",
                        "ProducedHeat": 2,
                        "ConsumedElectricity": 0,
                        "Cost": 1200,
                        "ProducedCO2": 800,
                        "AdditionalResources": {
                            "gas": 3.8
                        }
                    }
                ]
            }
        ]
        """.Replace(" ", "").Replace("\n", "").Replace("\r", "");
        string actual = resultDataManager.JsonExport();
        Assert.Equal(expected, actual);
    }
}