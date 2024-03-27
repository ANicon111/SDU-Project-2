namespace HeatManagement.tests;

public class SourceDataManagerTests
{

    //Testing json consistency
    [Fact]
    public void JsonTest()
    {
        string expected = """
        [
            {
                "StartTime": "2023-07-08T00:00:00",
                "EndTime": "2023-07-08T01:00:00",
                "HeatDemand": 1.79,
                "ElectricityPrice": 752.03
            },
            {
                "StartTime": "2023-07-08T01:00:00",
                "EndTime": "2023-07-08T02:00:00",
                "HeatDemand": 1.85,
                "ElectricityPrice": 691.05
            },
            {
                "StartTime": "2023-07-08T02:00:00",
                "EndTime": "2023-07-08T03:00:00",
                "HeatDemand": 1.76,
                "ElectricityPrice": 674.78
            },
            {
                "StartTime": "2023-07-08T03:00:00",
                "EndTime": "2023-07-08T04:00:00",
                "HeatDemand": 1.67,
                "ElectricityPrice": 652.95
            },
            {
                "StartTime": "2023-07-08T04:00:00",
                "EndTime": "2023-07-08T05:00:00",
                "HeatDemand": 1.73,
                "ElectricityPrice": 666.3
            },
            {
                "StartTime": "2023-07-08T05:00:00",
                "EndTime": "2023-07-08T06:00:00",
                "HeatDemand": 1.79,
                "ElectricityPrice": 654.6
            },
            {
                "StartTime": "2023-07-08T06:00:00",
                "EndTime": "2023-07-08T07:00:00",
                "HeatDemand": 1.82,
                "ElectricityPrice": 637.05
            },
            {
                "StartTime": "2023-07-08T07:00:00",
                "EndTime": "2023-07-08T08:00:00",
                "HeatDemand": 1.81,
                "ElectricityPrice": 639.45
            },
            {
                "StartTime": "2023-07-08T08:00:00",
                "EndTime": "2023-07-08T09:00:00",
                "HeatDemand": 1.84,
                "ElectricityPrice": 628.28
            },
            {
                "StartTime": "2023-07-08T09:00:00",
                "EndTime": "2023-07-08T10:00:00",
                "HeatDemand": 1.88,
                "ElectricityPrice": 570.3
            }
        ]
        """.Replace(" ", "").Replace("\n", "").Replace("\r", "");
        string actual = new SourceDataManager(expected).ToJson();
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
                "StartTime": "2023-07-08T00:00:00",
                "EndTime": "2023-07-08T01:00:00",
                "HeatDemand": 1.79,
                "ElectricityPrice": 752.03
            }
        ]
        """.Replace(" ", "").Replace("\n", "").Replace("\r", "");
        SourceDataManager sourceDataManager = new();
        sourceDataManager.AddData(
            new(
                startTime: new(year: 2023, month: 7, day: 8, hour: 0, minute: 0, second: 0),
                endTime: new(year: 2023, month: 7, day: 8, hour: 1, minute: 0, second: 0),
                heatDemand: 1.79,
                electricityPrice: 752.03
            )
        );
        string actual = sourceDataManager.ToJson();
        Assert.Equal(expected, actual);
    }


    //Testing data removal
    [Fact]
    public void RemovalTest()
    {
        string expected =
        """
        [
            {
                "StartTime": "2023-07-08T00:00:00",
                "EndTime": "2023-07-08T01:00:00",
                "HeatDemand": 1.79,
                "ElectricityPrice": 752.03
            }
        ]
        """.Replace(" ", "").Replace("\n", "").Replace("\r", "");
        SourceDataManager sourceDataManager = new(
        """
        [
            {
                "StartTime": "2023-07-08T00:00:00",
                "EndTime": "2023-07-08T01:00:00",
                "HeatDemand": 1.79,
                "ElectricityPrice": 752.03
            },
            {
                "StartTime": "2023-07-08T01:00:00",
                "EndTime": "2023-07-08T02:00:00",
                "HeatDemand": 1.85,
                "ElectricityPrice": 691.05
            }
        ]
        """.Replace(" ", "").Replace("\n", "").Replace("\r", "")
        );
        sourceDataManager.RemoveData(
            startTime: new DateTime(year: 2023, month: 7, day: 8, hour: 1, minute: 0, second: 0),
            endTime: new DateTime(year: 2023, month: 7, day: 8, hour: 2, minute: 0, second: 0)
        );
        string actual = sourceDataManager.ToJson();
        Assert.Equal(expected, actual);
    }
}
