namespace HeatManagement.tests;

public class Debug
{
    //Testing unit testing
    [Fact]
    public void UnitTestTest()
    {
        string expected = "test";
        string actual = HeatManagement.Debug.UnitTestTest();
        Assert.Equal(expected, actual);
    }
}