namespace HeatManagement.tests;

public class ArgumentsTests
{
    [Fact]
    public void Help()
    {
        Arguments actual = new(["-h"]);
        Assert.True(actual.Help);

        actual = new(["--help"]);
        Assert.True(actual.Help);

        actual = new(["--help", "-h", "-c"]);
        Assert.True(actual.Help);
    }

    [Fact]
    public void CLI()
    {
        Arguments actual = new(["-c"]);
        Assert.True(actual.CLIMode);

        actual = new(["--cli"]);
        Assert.True(actual.CLIMode);

        actual = new(["--gui", "-h", "-c"]);
        Assert.True(actual.CLIMode);
    }

    [Fact]
    public void GUI()
    {
        Arguments actual = new(["-g"]);
        Assert.True(actual.AvaloniaMode);

        actual = new(["--gui"]);
        Assert.True(actual.AvaloniaMode);

        actual = new(["--cli", "-h", "-g"]);
        Assert.True(actual.AvaloniaMode);
    }

    [Fact]
    public void AssetPath()
    {
        Arguments actual = new(["-a", "path"]);
        Assert.Equal("path", actual.AssetsPath);

        actual = new(["--assets", "path"]);
        Assert.Equal("path", actual.AssetsPath);

        actual = new(["--cli", "-h", "--assets", "path"]);
        Assert.Equal("path", actual.AssetsPath);

        try
        {
            actual = new(["-a"]);
        }
        catch (Exception e)
        {
            Assert.Equal("Expected an asset file path after -a", e.Message);
            return;
        }
        throw new("Failed to catch a missing file");
    }

    [Fact]
    public void DataPath()
    {
        Arguments actual = new(["-d", "path"]);
        Assert.Equal("path", actual.DataPath);

        actual = new(["--data", "path"]);
        Assert.Equal("path", actual.DataPath);

        actual = new(["--cli", "-h", "--data", "path"]);
        Assert.Equal("path", actual.DataPath);

        try
        {
            actual = new(["-d"]);
        }
        catch (Exception e)
        {
            Assert.Equal("Expected a source or result data file path after -d", e.Message);
            return;
        }
        throw new("Failed to catch a missing file");
    }
}