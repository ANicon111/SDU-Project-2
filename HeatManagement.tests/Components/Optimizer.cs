using System.Diagnostics.CodeAnalysis;

namespace HeatManagement.tests;

public class OptimizerTests
{
    [Fact]
    public void OptimalTest()
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
            });

        SourceDataManager sourceDataManager = new();
        sourceDataManager.DataAdd(
            startTime: new DateTime(year: 2023, month: 07, day: 08, hour: 0, minute: 0, second: 0),
            endTime: new DateTime(year: 2023, month: 07, day: 08, hour: 1, minute: 0, second: 0),
            heatDemand: 2,
            electricityPrice: 750
        );

        ResultDataManager expectedResultDataManager = new();
        expectedResultDataManager.DataAdd(new(
            startTime: new DateTime(year: 2023, month: 07, day: 08, hour: 0, minute: 0, second: 0),
            endTime: new DateTime(year: 2023, month: 07, day: 08, hour: 1, minute: 0, second: 0),
            results: [
                new(
                    unit:"GB",
                    producedHeat:2,
                    consumedElectricity:0,
                    cost:1000,
                    producedCO2:430,
                    additionalResources:new(){
                        {"gas",2.2}
                    }
                )
            ]
        ));

        ResultDataManager actualResultDataManager = new();
        new Optimizer(assetManager, sourceDataManager, actualResultDataManager).Optimize();

        Assert.Equal(expectedResultDataManager.JsonExport(), actualResultDataManager.JsonExport());
    }
}