namespace HeatManagement.tests;

public class ResultDataManagerTests
{

    //Testing json consistency
    [Fact]
    public void JsonTest()
    {
        string expected =
        """
        {
            "{\u0022Item1\u0022:\u00222023-02-14T19:00:00\u0022,\u0022Item2\u0022:\u00222023-02-14T20:00:00\u0022}": {
                "GM": {
                    "Heat": 3.6,
                    "Cost": 79.1819999999996,
                    "Electricity": 2.7,
                    "CO2": 2304,
                    "AdditionalResources": {
                        "gas": 6.84
                    }
                },
                "GB": {
                    "Heat": 2.78,
                    "Cost": 1390,
                    "Electricity": 0,
                    "CO2": 597.6999999999999,
                    "AdditionalResources": {
                        "gas": 3.058
                    }
                }
            },
            "{\u0022Item1\u0022:\u00222023-02-14T20:00:00\u0022,\u0022Item2\u0022:\u00222023-02-14T21:00:00\u0022}": {
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
            },
            "{\u0022Item1\u0022:\u00222023-02-14T21:00:00\u0022,\u0022Item2\u0022:\u00222023-02-14T22:00:00\u0022}": {
                "GM": {
                    "Heat": 3.6,
                    "Cost": 740.4389999999996,
                    "Electricity": 2.7,
                    "CO2": 2304,
                    "AdditionalResources": {
                        "gas": 6.84
                    }
                },
                "GB": {
                    "Heat": 2.7399999999999998,
                    "Cost": 1369.9999999999998,
                    "Electricity": 0,
                    "CO2": 589.0999999999999,
                    "AdditionalResources": {
                        "gas": 3.014
                    }
                }
            },
            "{\u0022Item1\u0022:\u00222023-02-14T22:00:00\u0022,\u0022Item2\u0022:\u00222023-02-14T23:00:00\u0022}": {
                "GM": {
                    "Heat": 3.6,
                    "Cost": 922.6349999999996,
                    "Electricity": 2.7,
                    "CO2": 2304,
                    "AdditionalResources": {
                        "gas": 6.84
                    }
                },
                "GB": {
                    "Heat": 2.81,
                    "Cost": 1405,
                    "Electricity": 0,
                    "CO2": 604.15,
                    "AdditionalResources": {
                        "gas": 3.091
                    }
                }
            },
            "{\u0022Item1\u0022:\u00222023-02-14T23:00:00\u0022,\u0022Item2\u0022:\u00222023-02-15T00:00:00\u0022}": {
                "GM": {
                    "Heat": 3.6,
                    "Cost": 1109.664,
                    "Electricity": 2.7,
                    "CO2": 2304,
                    "AdditionalResources": {
                        "gas": 6.84
                    }
                },
                "GB": {
                    "Heat": 2.7499999999999996,
                    "Cost": 1374.9999999999998,
                    "Electricity": 0,
                    "CO2": 591.2499999999999,
                    "AdditionalResources": {
                        "gas": 3.025
                    }
                }
            }
        }
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
        {
            "{\u0022Item1\u0022:\u00222023-02-14T19:00:00\u0022,\u0022Item2\u0022:\u00222023-02-14T20:00:00\u0022}": {
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
        string expectedGMRemoved =
        """
        {
            "{\u0022Item1\u0022:\u00222023-02-14T19:00:00\u0022,\u0022Item2\u0022:\u00222023-02-14T20:00:00\u0022}": {
                "GM": {
                    "Heat": 3.6,
                    "Cost": 79.1819999999996,
                    "Electricity": 2.7,
                    "CO2": 2304,
                    "AdditionalResources": {
                        "gas": 6.84
                    }
                }
            },
            "{\u0022Item1\u0022:\u00222023-02-14T20:00:00\u0022,\u0022Item2\u0022:\u00222023-02-14T21:00:00\u0022}": {
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
        """.Replace(" ", "").Replace("\n", "").Replace("\r", "");
        string expectedTimeRemoved =
        """
        {
            "{\u0022Item1\u0022:\u00222023-02-14T19:00:00\u0022,\u0022Item2\u0022:\u00222023-02-14T20:00:00\u0022}": {
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
        """.Replace(" ", "").Replace("\n", "").Replace("\r", "");
        ResultDataManager resultDataManager = new(
        """
        {
            "{\u0022Item1\u0022:\u00222023-02-14T19:00:00\u0022,\u0022Item2\u0022:\u00222023-02-14T20:00:00\u0022}": {
                "GM": {
                    "Heat": 3.6,
                    "Cost": 79.1819999999996,
                    "Electricity": 2.7,
                    "CO2": 2304,
                    "AdditionalResources": {
                        "gas": 6.84
                    }
                }
            },
            "{\u0022Item1\u0022:\u00222023-02-14T20:00:00\u0022,\u0022Item2\u0022:\u00222023-02-14T21:00:00\u0022}": {
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
        """
        );
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
