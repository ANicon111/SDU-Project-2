namespace HeatManagement.tests;

public class UnitTest1
{
    [Fact]
    public void UnitTestTest()
    {
        string expected = "test";
        string actual = HeatManagement.Debug.UnitTestTest();
        Assert.Equal(expected, actual);
    }
}