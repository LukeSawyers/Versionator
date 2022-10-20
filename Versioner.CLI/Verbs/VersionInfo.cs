using CommandLine;

[Verb("version-info", aliases: new[] { "--vi" }, HelpText = "Print the version for a particular version")]
public record VersionInfo : VerbBase
{
}