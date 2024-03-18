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
                    "gas": 1.1
                }
            },
            "OB": {
                "ImagePath": "Assets/OB.png",
                "HeatCapacity": 4,
                "Cost": 700,
                "ElectricityCapacity": 0,
                "CO2": 265,
                "AdditionalResources": {
                    "oil": 1.2
                }
            }
        }
        """.Replace(" ", "").Replace("\n", "").Replace("\r", "");
        string actual = new AssetManager(expected).ToJson();
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
                    "gas": 1.1
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
                    {"gas",1.1}
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
                    "gas": 1.1
                }
            }
        }
        """.Replace(" ", "").Replace("\n", "").Replace("\r", "");
        AssetManager assetManager = new(
            """
        {
            "GB": {
                "ImagePath": "Assets/GB.png",
                "HeatCapacity": 5,
                "Cost": 500,
                "ElectricityCapacity": 0,
                "CO2": 215,
                "AdditionalResources": {
                    "gas": 1.1
                }
            },
            "OB": {
                "ImagePath": "Assets/OB.png",
                "HeatCapacity": 4,
                "Cost": 700,
                "ElectricityCapacity": 0,
                "CO2": 265,
                "AdditionalResources": {
                    "oil": 1.2
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