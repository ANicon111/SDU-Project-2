namespace HeatManagement.tests;

public class ResultDataManagerTests
{

    //Testing json consistency
    [Fact]
    public void JsonTest()
    {
        string expected =
        """
        [
            {
                "StartTime": "2023-02-08T00:00:00",
                "EndTime": "2023-02-08T01:00:00",
                "ResultData": {
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
                }
            },
            {
                "StartTime": "2023-02-08T01:00:00",
                "EndTime": "2023-02-08T02:00:00",
                "ResultData": {
                    "B": {
                        "Heat": 4,
                        "Cost": 1000,
                        "Electricity": -8,
                        "CO2": 0,
                        "AdditionalResources": {
                            "R2": 4
                        }
                    }
                }
            },
            {
                "StartTime": "2023-02-08T02:00:00",
                "EndTime": "2023-02-08T03:00:00",
                "ResultData": {
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
        ]
        """.Replace(" ", "").Replace("\n", "").Replace("\r", "");
        string actual = new ResultDataManager(expected).ToJson();
        Assert.Equal(expected, actual);
    }

    //Testing data addition
    [Fact]
    public void AdditionTest()
    {
        string expected =
        """
        [
            {
                "StartTime": "2023-02-14T19:00:00",
                "EndTime": "2023-02-14T20:00:00",
                "ResultData": {
                    "GM": {
                        "Heat": 3.6,
                        "Cost": 79.1819999999996,
                        "Electricity": 2.7,
                        "CO2": 2304,
                        "AdditionalResources": {
                            "gas": 6.84
                        }
                    }
                }
            }
        ]
        """.Replace(" ", "").Replace("\n", "").Replace("\r", "");
        ResultDataManager resultDataManager = new();
        resultDataManager.AddData(
            startTime: new(year: 2023, month: 2, day: 14, hour: 19, minute: 0, second: 0),
            endTime: new(year: 2023, month: 2, day: 14, hour: 20, minute: 0, second: 0),
            assetName: "GM",
            assetData: new(
                heat: 3.6,
                cost: 79.1819999999996,
                electricity: 2.7,
                co2: 2304,
                additionalResources: new(){
                    {"gas",6.84}
                }
            )
        );
        string actual = resultDataManager.ToJson();
        Assert.Equal(expected, actual);
    }

    //Testing data removal
    [Fact]
    public void RemovalTest()
    {
        ResultDataManager resultDataManager = new(
            """
            [
                {
                    "StartTime": "2023-02-14T19:00:00",
                    "EndTime": "2023-02-14T20:00:00",
                    "ResultData": {
                        "GM": {
                            "Heat": 3.6,
                            "Cost": 79.1819999999996,
                            "Electricity": 2.7,
                            "CO2": 2304,
                            "AdditionalResources": {
                                "gas": 6.84
                            }
                        }
                    }
                },
                {
                    "StartTime": "2023-02-14T20:00:00",
                    "EndTime": "2023-02-14T21:00:00",
                    "ResultData": {
                        "GM": {
                            "Heat": 3.6,
                            "Cost": 424.29600000000005,
                            "Electricity": 2.7,
                            "CO2": 2304,
                            "AdditionalResources": {
                                "gas": 6.84
                            }
                        },
                        "GB": {
                            "Heat": 2.9099999999999997,
                            "Cost": 1454.9999999999998,
                            "Electricity": 0,
                            "CO2": 625.65,
                            "AdditionalResources": {
                                "gas": 3.201
                            }
                        }
                    }
                }
            ]
            """
        );
        string expectedGMRemoved =
            """
            [
                {
                    "StartTime": "2023-02-14T19:00:00",
                    "EndTime": "2023-02-14T20:00:00",
                    "ResultData": {
                        "GM": {
                            "Heat": 3.6,
                            "Cost": 79.1819999999996,
                            "Electricity": 2.7,
                            "CO2": 2304,
                            "AdditionalResources": {
                                "gas": 6.84
                            }
                        }
                    }
                },
                {
                    "StartTime": "2023-02-14T20:00:00",
                    "EndTime": "2023-02-14T21:00:00",
                    "ResultData": {
                        "GB": {
                            "Heat": 2.9099999999999997,
                            "Cost": 1454.9999999999998,
                            "Electricity": 0,
                            "CO2": 625.65,
                            "AdditionalResources": {
                                "gas": 3.201
                            }
                        }
                    }
                }
            ]
            """.Replace(" ", "").Replace("\n", "").Replace("\r", "");
        string expectedTimeRemoved =
            """
            [
                {
                    "StartTime": "2023-02-14T19:00:00",
                    "EndTime": "2023-02-14T20:00:00",
                    "ResultData": {
                        "GM": {
                            "Heat": 3.6,
                            "Cost": 79.1819999999996,
                            "Electricity": 2.7,
                            "CO2": 2304,
                            "AdditionalResources": {
                                "gas": 6.84
                            }
                        }
                    }
                }
            ]
            """.Replace(" ", "").Replace("\n", "").Replace("\r", "");
        resultDataManager.RemoveData(
            startTime: new(year: 2023, month: 2, day: 14, hour: 20, minute: 0, second: 0),
            endTime: new(year: 2023, month: 2, day: 14, hour: 21, minute: 0, second: 0),
            assetName: "GM"
        );
        string actual = resultDataManager.ToJson();
        Assert.Equal(expectedGMRemoved, actual);
        resultDataManager.RemoveData(
            startTime: new(year: 2023, month: 2, day: 14, hour: 20, minute: 0, second: 0),
            endTime: new(year: 2023, month: 2, day: 14, hour: 21, minute: 0, second: 0)
        );
        actual = resultDataManager.ToJson();
        Assert.Equal(expectedTimeRemoved, actual);
    }
}
