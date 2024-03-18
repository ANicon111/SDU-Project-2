namespace HeatManagement.tests;

public class OptimizerTests
{

    //Testing if the optimizer gives optimal results
    [Fact]
    public void OptimalTest()
    {
        string expected = new ResultDataManager(
            """
            {
                "{\u0022Item1\u0022:\u00222023-02-08T00:00:00\u0022,\u0022Item2\u0022:\u00222023-02-08T01:00:00\u0022}": {
                    "C": {
                        "Heat": 4,
                        "Cost": 400,
                        "Electricity": 4,
                        "CO2": 8000,
                        "AdditionalResources": {
                            "R1": 8
                        }
                    },
                    "A": {
                        "Heat": 5,
                        "Cost": 2500,
                        "Electricity": 0,
                        "CO2": 5000,
                        "AdditionalResources": {
                            "R1": 5
                        }
                    },
                    "B": {
                        "Heat": 1,
                        "Cost": 2050,
                        "Electricity": -2,
                        "CO2": 0,
                        "AdditionalResources": {
                            "R2": 1
                        }
                    }
                },
                "{\u0022Item1\u0022:\u00222023-02-08T01:00:00\u0022,\u0022Item2\u0022:\u00222023-02-08T02:00:00\u0022}": {
                    "B": {
                        "Heat": 4,
                        "Cost": 1000,
                        "Electricity": -8,
                        "CO2": 0,
                        "AdditionalResources": {
                            "R2": 4
                        }
                    }
                },
                "{\u0022Item1\u0022:\u00222023-02-08T02:00:00\u0022,\u0022Item2\u0022:\u00222023-02-08T03:00:00\u0022}": {
                    "A": {
                        "Heat": 5,
                        "Cost": 2500,
                        "Electricity": 0,
                        "CO2": 5000,
                        "AdditionalResources": {
                            "R1": 5
                        }
                    },
                    "C": {
                        "Heat": 3,
                        "Cost": 1800,
                        "Electricity": 3,
                        "CO2": 6000,
                        "AdditionalResources": {
                            "R1": 6
                        }
                    }
                }
            }
            """).ToJson();

        AssetManager assetManager = new(
            """
            {
                "A": {
                    "ImagePath": "",
                    "HeatCapacity": 5,
                    "Cost": 500,
                    "ElectricityCapacity": 0,
                    "CO2": 1000,
                    "AdditionalResources": {
                        "R1": 1
                    }
                },
                "B": {
                    "ImagePath": "",
                    "HeatCapacity": 4,
                    "Cost": 50,
                    "ElectricityCapacity": -8,
                    "CO2": 0,
                    "AdditionalResources": {
                        "R2": 1
                    }
                },
                "C": {
                    "ImagePath": "",
                    "HeatCapacity": 4,
                    "Cost": 1100,
                    "ElectricityCapacity": 4,
                    "CO2": 2000,
                    "AdditionalResources": {
                        "R1": 2
                    }
                }
            }
            """
        );

        SourceDataManager sourceDataManager = new(
            """
            [
                {
                    "StartTime": "2023-02-08T00:00:00",
                    "EndTime": "2023-02-08T01:00:00",
                    "HeatDemand": 10,
                    "ElectricityPrice": 1000
                },
                {
                    "StartTime": "2023-02-08T01:00:00",
                    "EndTime": "2023-02-08T02:00:00",
                    "HeatDemand": 4,
                    "ElectricityPrice": 100
                },
                {
                    "StartTime": "2023-02-08T02:00:00",
                    "EndTime": "2023-02-08T03:00:00",
                    "HeatDemand": 8,
                    "ElectricityPrice": 500
                }
            ]
            """
        );

        ResultDataManager resultDataManager = new();

        Optimizer.GetResult(assetManager, sourceDataManager, resultDataManager);

        string actual = resultDataManager.ToJson();

        Assert.Equal(expected, actual);
    }

}