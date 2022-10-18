using CommandLine;

[Verb("check-out", HelpText = "Check out a specific version of a file")]
public class CheckOut : VerbBase
{
    [Option('v', Required = true, HelpText = "The version to check out")]
    public string Version { get; set; }
}