namespace HeatManagement.tests;

public class DataGetter
{
    [Fact]
    public void Test()
    {
        List<SourceData> data = Utils.GetSourceDataFromAPI(new(year: 2024, month: 1, day: 1, hour: 0, minute: 0, second: 0), new(year: 2024, month: 1, day: 1, hour: 1, minute: 0, second: 0));
        Assert.NotEmpty(data);
    }
}