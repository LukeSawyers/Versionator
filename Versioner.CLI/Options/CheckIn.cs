using CommandLine;

[Verb("--check-in")]
public class CheckIn : VerbBase
{
    [Option('v', Required = false)] public string? Version { get; set; }
    
    [Option('c', Required = false)] public string UnknownChanges { get; set; } = "";

    [Option('a', Required = false)] public string AdditionChanges { get; set; } = "";

    [Option('m', Required = false)] public string ModificationChanges { get; set; } = "";

    [Option('d', Required = false)] public string DeletionChanges { get; set; } = "";
}