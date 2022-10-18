using CommandLine;

[Verb("list-versions", HelpText = "List all available versions of a file")]
public class ListVersions : VerbBase
{
    [Option('v', Required = false, HelpText = "The version to check in. If blank the current version will be used")]
    public string? Version { get; set; }

    [Option('c', Required = false, HelpText = "Misc changes that were made in this version. Semi-colon separated")]
    public string UnknownChanges { get; set; } = "";

    [Option('a', Required = false, HelpText = "Additions that were made in this version. Semi-colon separated")]
    public string AdditionChanges { get; set; } = "";

    [Option('m', Required = false, HelpText = "Modifications that were made in this version. Semi-colon separated")]
    public string ModificationChanges { get; set; } = "";

    [Option('d', Required = false, HelpText = "Deletions that were made in this version. Semi-colon separated")]
    public string DeletionChanges { get; set; } = "";
}