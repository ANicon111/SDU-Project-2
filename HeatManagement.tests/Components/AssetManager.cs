namespace HeatManagement.tests;

public class AssetManagerTests
{
    [Fact]
    public void JsonTest()
    {
        string expected = """
        [
            {
                "Name": "GB",
                "ImagePath": "Assets/GB.png",
                "HeatCapacity": 5,
                "Cost": 500,
                "ElectricityCapacity": 0,
                "CO2": 215,
                "AdditionalResources": {
                    "gas": 1.1
                }
            },
            {
                "Name": "OB",
                "ImagePath": "Assets/OB.png",
                "HeatCapacity": 4,
                "Cost": 700,
                "ElectricityCapacity": 0,
                "CO2": 265,
                "AdditionalResources": {
                    "oil": 1.2
                }
            }
        ]
        """.Replace(" ", "").Replace("\n", "").Replace("\r", "");
        AssetManager assetManager = new();
        assetManager.JsonImport(expected);
        string actual = assetManager.JsonExport();
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void AdditionTest()
    {
        AssetManager assetManager = new();
        assetManager.DataAdd(
            name: "GB",
            imagePath: "Assets/GB.png",
            heatCapacity: 5,
            cost: 500,
            electricityCapacity: 0,
            co2: 215,
            additionalResources: new Dictionary<string, double>(){
                {"gas",1.1}
            }
        );
        string expected = """
        [
            {
                "Name": "GB",
                "ImagePath": "Assets/GB.png",
                "HeatCapacity": 5,
                "Cost": 500,
                "ElectricityCapacity": 0,
                "CO2": 215,
                "AdditionalResources": {
                    "gas": 1.1
                }
            }
        ]
        """.Replace(" ", "").Replace("\n", "").Replace("\r", "");
        string actual = assetManager.JsonExport();
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void RemovalTest()
    {
        AssetManager assetManager = new();
        assetManager.JsonImport("""
         [
            {
                "Name": "GB",
                "ImagePath": "Assets/GB.png",
                "HeatCapacity": 5,
                "Cost": 500,
                "ElectricityCapacity": 0,
                "CO2": 215,
                "AdditionalResources": {
                    "gas": 1.1
                }
            },
            {
                "Name": "OB",
                "ImagePath": "Assets/OB.png",
                "HeatCapacity": 4,
                "Cost": 700,
                "ElectricityCapacity": 0,
                "CO2": 265,
                "AdditionalResources": {
                    "oil": 1.2
                }
            }
        ]
        """);
        assetManager.DataRemove(
            name: "OB"
        );
        string expected = """
        [
            {
                "Name": "GB",
                "ImagePath": "Assets/GB.png",
                "HeatCapacity": 5,
                "Cost": 500,
                "ElectricityCapacity": 0,
                "CO2": 215,
                "AdditionalResources": {
                    "gas": 1.1
                }
            }
        ]
        """.Replace(" ", "").Replace("\n", "").Replace("\r", "");
        string actual = assetManager.JsonExport();
        Assert.Equal(expected, actual);
    }


}