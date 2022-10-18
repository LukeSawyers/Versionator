using CommandLine;

public abstract class VerbBase
{
    [Option('f', Required = true, HelpText = "The file to version control")]
    public string File { get; set; }
}