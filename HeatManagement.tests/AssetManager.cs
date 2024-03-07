namespace HeatManagement.tests;

public class AssetManager
{
    [Fact]
    public void Json()
    {
        string expected = "test";
        string actual = HeatManagement.Debug.UnitTestTest();
        Assert.Equal(expected, actual);
    }
}