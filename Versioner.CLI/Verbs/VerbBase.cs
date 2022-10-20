using CommandLine;

public abstract record VerbBase
{
    [Option('f', Required = false, HelpText = "The file to version control")]
    public string File { get; set; } = "";
}