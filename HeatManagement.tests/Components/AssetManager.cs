using System.Globalization;

namespace HeatManagement.tests;

public class AssetManagerTests
{

    //Testing json consistency
    [Fact]
    public void JsonTest()
    {

        string expected = """
        {
            "GB": {
                "ImagePath": "Assets/GB.png",
                "HeatCapacity": 5,
                "Cost": 500,
                "ElectricityCapacity": 0,
                "CO2": 215,
                "AdditionalResources": {
                    "gas": {
                        "Measurement": "MWh",
                        "Value": -1.1
                    }
                }
            },
            "OB": {
                "ImagePath": "Assets/OB.png",
                "HeatCapacity": 4,
                "Cost": 700,
                "ElectricityCapacity": 0,
                "CO2": 265,
                "AdditionalResources": {
                    "oil": {
                        "Measurement": "MWh",
                        "Value": -1.2
                    }
                }
            }
        }
        """.Replace(" ", "").Replace("\n", "").Replace("\r", "");
        string actual = AssetManager.FromJson(expected).ToJson();
        Assert.Equal(expected, actual);
    }

    //Testing csv compatibility
    [Fact]
    public void CSVTest()
    {
        CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
        string expected = """
        Name,DataType,ImagePath,HeatCapacity,Cost,ElectricityCapacity,CO2,AdditionalResourceName,AdditionalResourceValue,AdditionalResourceMeasurement
        GB,Base,Assets/GB.png,5,500,0,215,,
        GB,Additional,,,,,,gas,-1.1,MWh
        OB,Base,Assets/OB.png,4,700,0,265,,
        OB,Additional,,,,,,oil,-1.2,MWh
        """;

        string json = """
        {
            "GB": {
                "ImagePath": "Assets/GB.png",
                "HeatCapacity": 5,
                "Cost": 500,
                "ElectricityCapacity": 0,
                "CO2": 215,
                "AdditionalResources": {
                    "gas": {
                        "Measurement": "MWh",
                        "Value": -1.1
                    }
                }
            },
            "OB": {
                "ImagePath": "Assets/OB.png",
                "HeatCapacity": 4,
                "Cost": 700,
                "ElectricityCapacity": 0,
                "CO2": 265,
                "AdditionalResources": {
                    "oil": {
                        "Measurement": "MWh",
                        "Value": -1.2
                    }
                }
            }
        }
        """.Replace(" ", "").Replace("\n", "").Replace("\r", "");
        string actual = AssetManager.FromJson(json).ToCSV();
        Assert.Equal(expected, actual);
    }

    //Testing data addition
    [Fact]
    public void AdditionTest()
    {
        string expected =
        """
        {
            "GB": {
                "ImagePath": "Assets/GB.png",
                "HeatCapacity": 5,
                "Cost": 500,
                "ElectricityCapacity": 0,
                "CO2": 215,
                "AdditionalResources": {
                    "gas": {
                        "Measurement": "MWh",
                        "Value": -1.1
                    }
                }
            }
        }
        """.Replace(" ", "").Replace("\n", "").Replace("\r", "");
        AssetManager assetManager = new();
        assetManager.AddAsset(
            "GB",
            new(
                imagePath: "Assets/GB.png",
                heatCapacity: 5,
                cost: 500,
                electricityCapacity: 0,
                co2: 215,
                additionalResources: new(){
                    {"gas",new(-1.1,"MWh")}
                }
            )
        );
        string actual = assetManager.ToJson();
        Assert.Equal(expected, actual);
    }

    //Testing data removal
    [Fact]
    public void RemovalTest()
    {
        string expected =
        """
        {
            "GB": {
                "ImagePath": "Assets/GB.png",
                "HeatCapacity": 5,
                "Cost": 500,
                "ElectricityCapacity": 0,
                "CO2": 215,
                "AdditionalResources": {
                    "gas": {
                        "Measurement": "MWh",
                        "Value": -1.1
                    }
                }
            }
        }
        """.Replace(" ", "").Replace("\n", "").Replace("\r", "");
        AssetManager assetManager = AssetManager.FromJson(
            """
        {
            "GB": {
                "ImagePath": "Assets/GB.png",
                "HeatCapacity": 5,
                "Cost": 500,
                "ElectricityCapacity": 0,
                "CO2": 215,
                "AdditionalResources": {
                    "gas": {
                        "Measurement": "MWh",
                        "Value": -1.1
                    }
                }
            },
            "OB": {
                "ImagePath": "Assets/OB.png",
                "HeatCapacity": 4,
                "Cost": 700,
                "ElectricityCapacity": 0,
                "CO2": 265,
                "AdditionalResources": {
                    "oil": {
                        "Measurement": "MWh",
                        "Value": -1.2
                    }
                }
            }
        }
        """.Replace(" ", "").Replace("\n", "").Replace("\r", "")
        );
        assetManager.RemoveAsset("OB");
        string actual = assetManager.ToJson();
        Assert.Equal(expected, actual);
    }
}