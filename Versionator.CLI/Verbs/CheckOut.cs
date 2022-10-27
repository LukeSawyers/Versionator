using CommandLine;
using Versionator.Core;

namespace Versionator.CLI.Verbs;

[Verb("check-out", aliases: new[] { "--co" }, HelpText = "Check out the specified version of the document")]
public record CheckOut : VerbBase
{
    [Option('v', Required = true)] public string Version { get; set; }

    public static async Task RunAsync(CheckOut args)
    {
        using var controller = await DocumentController.CreateAsync(args.File);
        if (controller == null)
        {
            return;
        }

        await RunAsync(controller, args);
    }
    
    
    public static async Task RunAsync(DocumentController controller, CheckOut args)
    {
        var options = await controller.Accessor.GetOptionsAsync();
        var version = DocumentVersion.Parse(options.VersioningType, args.Version);
        await controller.CheckOutVersionAsync(version);
        
        Console.WriteLine($"Checked out {Path.GetFileName(args.File)} version {version.ToVersionString()}");
    }
}