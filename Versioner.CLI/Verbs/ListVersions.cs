using CommandLine;
using Versioner.Core;

[Verb("list-versions", aliases: new[] { "--lv" }, HelpText = "List all available versions of a file")]
public record ListVersions : VerbBase
{
    [Option('a', HelpText = "List all info for each version")]
    public bool All { get; set; }

    public static async Task RunAsync(ListVersions args)
    {
        var controller = await DocumentController.CreateAsync(args.File);
        if (controller == null)
        {
            return;
        }

        var index = await controller.Accessor.GetVersionIndexAsync();
        var versions = index
            .Select(p => (p.Key.ToVersionString(), p.Value))
            .OrderByDescending(v => v)
            .ToArray();

        var currentVersion = (await controller.Accessor.GetCurrentVersionAsync()).ToVersionString();

        foreach (var (version, info) in versions)
        {
            var currentIndicator = version == currentVersion ? "**" : "  ";
            var versionStr = info.CommittedDate != null ? $"[{version}]" : $"({version})";
            Console.WriteLine(currentIndicator + versionStr);
        }
    }
}