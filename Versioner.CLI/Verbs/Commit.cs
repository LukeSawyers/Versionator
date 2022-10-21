
using CommandLine;
using Versioner.Core;

[Verb("commit", aliases: new[] { "--cm" })]
public record Commit : VerbBase
{
    [Option('v', Required = false)] public string? Version { get; set; }
    
    public static async Task RunAsync(Commit args)
    {
        using var controller = await DocumentController.CreateAsync(args.File);
        if (controller == null)
        {
            return;
        }

        await RunAsync(controller, args);
    }

    public static async Task RunAsync(DocumentController controller, Commit commit)
    {
        var options = await controller.Accessor.GetOptionsAsync();

        var version = commit.Version == null
            ? await controller.Accessor.GetCurrentVersionAsync()
            : DocumentVersion.Parse(options.VersioningType, commit.Version);

        var result = await controller.CommitVersionAsync(version);

        var print = result switch
        {
            CommitResult.Ok => $"Committed version {version.ToVersionString()}",
            CommitResult.AlreadyCommitted => $"Version {version.ToVersionString()} was already committed",
            CommitResult.NoSuchVersion => $"No such version {version.ToVersionString()}",
            _ => throw new ArgumentOutOfRangeException()
        };

        Console.WriteLine(print);
    }
}