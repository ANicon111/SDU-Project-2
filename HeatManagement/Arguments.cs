namespace HeatManagement;

class Arguments
{
    public bool CLIMode { get; } = false;
    public bool AvaloniaMode { get; } = true;
    public bool Help { get; } = false;
    public string? AssetsPath { get; init; } = null;
    public string? DataPath { get; init; } = null;
    public string? EditPath { get; init; } = null;

    public static string HelpMessage =
        """
        Syntax:
        heatmanagement [OPTIONS]

        -h, --help: Show this message
        -c, --cli: Run the command line visualizer
        -g, --gui: Run the graphical visualizer
        -d, --data <path>: Set initial source or result data file path
        -a, --assets <path>: Set initial assets file path
        -e, --edit <path>: Edit an asset or source data file
        """;

    public Arguments(string[] args)
    {
        for (int i = 0; i < args.Length; i++)
        {
            switch (args[i])
            {
                case "-c":
                case "--cli":
                    CLIMode = true;
                    AvaloniaMode = false;
                    break;
                case "-g":
                case "--gui":
                    AvaloniaMode = true;
                    CLIMode = false;
                    break;
                case "-h":
                case "--help":
                    Help = true;
                    break;
                case "-a":
                case "--assets":
                    i++;
                    try { AssetsPath = args[i]; } catch { throw new($"Expected an asset file path after {args[i - 1]}"); }
                    break;

                case "-d":
                case "--data":
                    i++;
                    try { DataPath = args[i]; } catch { throw new($"Expected a source or result data file path after {args[i - 1]}"); }
                    break;

                case "-e":
                case "--edit":
                    i++;
                    try { DataPath = args[i]; } catch { throw new($"Expected an asset or source data file path after {args[i - 1]}"); }
                    break;

                //handle accidental double spaces
                case "":
                    break;
                default:
                    throw new($"Invalid argument: {args[i]}");
            }
        }
    }
}