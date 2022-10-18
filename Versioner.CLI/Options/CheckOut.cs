
using CommandLine;

[Verb("--check-out")]
public class CheckOut : VerbBase
{
    [Option('v', Required = true)] public string Version { get; set; }
}