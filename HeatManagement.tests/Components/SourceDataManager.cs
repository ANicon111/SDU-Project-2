namespace HeatManagement.tests;

public class SourceDataManagerTests
{
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
            }
        ]
        """.Replace(" ", "").Replace("\n", "").Replace("\r", "");
        SourceDataManager sourceDataManager = new();
        sourceDataManager.JsonImport(expected);
        string actual = sourceDataManager.JsonExport();
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void AdditionTest()
    {
        SourceDataManager sourceDataManager = new();
        sourceDataManager.DataAdd(
            startTime: new DateTime(year: 2023, month: 07, day: 08, hour: 0, minute: 0, second: 0),
            endTime: new DateTime(year: 2023, month: 07, day: 08, hour: 1, minute: 0, second: 0),
            heatDemand: 1.79,
            electricityPrice: 752.03
        );

        string expected = """
        [
            {
        "StartTime": "2023-07-08T00:00:00",
        "EndTime": "2023-07-08T01:00:00",
        "HeatDemand": 1.79,
        "ElectricityPrice": 752.03
            }
        ]
        """.Replace(" ", "").Replace("\n", "").Replace("\r", "");

        string actual = sourceDataManager.JsonExport();
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void RemovalTest()
    {
        string expected = """
        [
            {
            "StartTime": "2023-02-08T01:00:00",
            "EndTime": "2023-02-08T02:00:00",
            "HeatDemand": 6.85,
            "ElectricityPrice": 1154.55
            }
        ]
        """.Replace(" ", "").Replace("\n", "").Replace("\r", "");
        SourceDataManager sourceDataManager = new();
        sourceDataManager.JsonImport("""
        [
            {
            "StartTime": "2023-02-08T01:00:00",
            "EndTime": "2023-02-08T02:00:00",
            "HeatDemand": 6.85,
            "ElectricityPrice": 1154.55
            },
            {
            "StartTime": "2023-02-08T02:00:00",
            "EndTime": "2023-02-08T03:00:00",
            "HeatDemand": 6.98,
            "ElectricityPrice": 1116.22
            }
        ]
        """);
        sourceDataManager.DataRemove(
            startTime: new DateTime(year: 2023, month: 02, day: 08, hour: 02, minute: 0, second: 0)
        );
        string actual = sourceDataManager.JsonExport();
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void CSVExportTest()
    {
        string expected = """
            StartTime,EndTime,HeatDemand,ElectricityPrice
            2023-07-08T00:00:00,2023-07-08T01:00:00,1.79,752.03
            2022-07-08T00:00:00,2022-07-08T01:00:00,1.05,743.03
            """.Replace("\r\n", "\n");
        SourceDataManager sourceDataManager = new();
        sourceDataManager.DataAdd(
            startTime: new DateTime(year: 2023, month: 07, day: 08, hour: 0, minute: 0, second: 0),
            endTime: new DateTime(year: 2023, month: 07, day: 08, hour: 1, minute: 0, second: 0),
            heatDemand: 1.79,
            electricityPrice: 752.03
        );
        sourceDataManager.DataAdd(
            startTime: new DateTime(year: 2022, month: 07, day: 08, hour: 0, minute: 0, second: 0),
            endTime: new DateTime(year: 2022, month: 07, day: 08, hour: 1, minute: 0, second: 0),
            heatDemand: 1.05,
            electricityPrice: 743.03
        );
        string actual = sourceDataManager.CSVExport();
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void CSVImportTest()
    {
        string data = """
            StartTime,EndTime,HeatDemand,ElectricityPrice
            07/08/2023 00:00:00,07/08/2023 01:00:00,1.79,752.03
            07/08/2022 00:00:00,07/08/2022 01:00:00,1.05,743.03
            """.Replace("\r\n", "\n");

        SourceDataManager sourceDataManager = new();
        SourceDataManager sourceDataManager1 = new();

        sourceDataManager.CSVImport(data);
        string actual = sourceDataManager.JsonExport();

        sourceDataManager1.DataAdd(
            startTime: new DateTime(year: 2023, month: 08, day: 07, hour: 0, minute: 0, second: 0),
            endTime: new DateTime(year: 2023, month: 08, day: 07, hour: 1, minute: 0, second: 0),
            heatDemand: 1.79,
            electricityPrice: 752.03
        );
        sourceDataManager1.DataAdd(
            startTime: new DateTime(year: 2022, month: 08, day: 07, hour: 0, minute: 0, second: 0),
            endTime: new DateTime(year: 2022, month: 08, day: 07, hour: 1, minute: 0, second: 0),
            heatDemand: 1.05,
            electricityPrice: 743.03
        );

        string expected = sourceDataManager1.JsonExport();

        Assert.Equal(expected, actual);
    }

}