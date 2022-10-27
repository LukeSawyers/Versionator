using CommandLine;
using Versioner.Core;

[Verb("check-in", aliases: new[] { "--ci" }, HelpText = "Check in specified version of the document")]
public record CheckIn : VerbBase
{
    [Option('v', Required = false, HelpText = "The version to check in. If blank the current version will be used")]
    public string? Version { get; set; }

    [Option('c', "unknown", Required = false,
        HelpText = "Misc changes that were made in this version. Semi-colon separated")]
    public IEnumerable<string> UnknownChanges { get; set; } = Array.Empty<string>();

    [Option('a', "added", Required = false,
        HelpText = "Additions that were made in this version. Semi-colon separated")]
    public IEnumerable<string> AdditionChanges { get; set; } = Array.Empty<string>();

    [Option('m', "changed", Required = false,
        HelpText = "Modifications that were made in this version. Semi-colon separated")]
    public IEnumerable<string> ModificationChanges { get; set; } = Array.Empty<string>();

    [Option('r', "fixed", Required = false, HelpText = "Fixups that were made in this version. Semi-colon separated")]
    public IEnumerable<string> FixupChanges { get; set; } = Array.Empty<string>();

    [Option('d', "deleted", Required = false,
        HelpText = "Deletions that were made in this version. Semi-colon separated")]
    public IEnumerable<string> DeletionChanges { get; set; } = Array.Empty<string>();

    public static async Task RunAsync(CheckIn args)
    {
        using var controller = await DocumentController.CreateAsync(args.File);
        if (controller == null)
        {
            return;
        }

        await RunAsync(controller, args);
    }

    public static async Task RunAsync(DocumentController controller, CheckIn args)
    {
        IEnumerable<ChangeLog> GetChanges(IEnumerable<string> tokens, ChangeType type)
        {
            var str = string.Join(' ', tokens);

            return str
                .Split(';', StringSplitOptions.RemoveEmptyEntries)
                .Select(c => c.TrimStart().TrimEnd())
                .Select(c => new ChangeLog(type, c));
        }

        var changes = Enumerable.Empty<ChangeLog>()
            .Concat(GetChanges(args.UnknownChanges, ChangeType.Unknown))
            .Concat(GetChanges(args.AdditionChanges, ChangeType.Addition))
            .Concat(GetChanges(args.ModificationChanges, ChangeType.Modification))
            .Concat(GetChanges(args.FixupChanges, ChangeType.Fixup))
            .Concat(GetChanges(args.DeletionChanges, ChangeType.Deletion))
            .ToArray();

        var options = await controller.Accessor.GetOptionsAsync();

        var version = args.Version == null
            ? null
            : DocumentVersion.Parse(options.VersioningType, args.Version);

        if (version == null)
        {
            var result = await controller.CheckInCurrentVersionAsync(changes);
            version = await controller.Accessor.GetCurrentVersionAsync();
            var print = result switch
            {
                CheckInResult.Ok => $"Checked in {Path.GetFileName(args.File)} version {version.ToVersionString()}",
                CheckInResult.Committed =>
                    $"Could not check in version {version.ToVersionString()} as the version has been committed. Consider checking in as a new version",
                null => "",
                _ => throw new ArgumentOutOfRangeException()
            };
            Console.WriteLine(print);
        }
        else
        {
            var result = await controller.CheckInVersionAsync(version, changes);
            var print = result switch
            {
                CheckInResult.Ok => $"Checked in {Path.GetFileName(args.File)} to version {version.ToVersionString()}",
                CheckInResult.Committed =>
                    $"Could not check in version {version.ToVersionString()} as the version has been committed. Consider checking in as a new version",
                _ => throw new ArgumentOutOfRangeException()
            };

            Console.WriteLine(print);
        }

        Console.WriteLine();
    }
}