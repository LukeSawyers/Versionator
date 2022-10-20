
using CommandLine;
using Versioner.Core;

[Verb("check-out", aliases: new[] { "--co" }, HelpText = "Check out the specified version of the document")]
public record CheckOut : VerbBase
{
    [Option('v', Required = true)] public string Version { get; set; }
    
    public static async Task RunAsync(CheckOut args)
    {
        var controller = await DocumentController.CreateAsync(args.File);
        if (controller == null)
        {
            return;
        }

        var options = await controller.Accessor.GetOptionsAsync();
        var version = DocumentVersion.Parse(options.VersioningType, args.Version);
        await controller.CheckOutVersionAsync(version);
    }
}