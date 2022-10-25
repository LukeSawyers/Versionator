using CommandLine;
using Versioner.Core;

[Verb("rename", aliases: new[] { "--rn" }, HelpText = "Rename the document")]
public record Rename : VerbBase
{
    [Option('n', Required = true, HelpText = "The new name for the document(s)")]
    public string NewName { get; set; }

    public static async Task RunAsync(Rename args)
    {
        using var controller = await DocumentController.CreateAsync(args.File);
        if (controller == null)
        {
            return;
        }

        await RunAsync(controller, args);
    }


    public static async Task RunAsync(DocumentController controller, Rename args)
    {
        var oldName = controller.Accessor.FileName; 
        await controller.RenameAsync(args.NewName);

        Console.WriteLine($"Renamed file {oldName} and all versions to match {args.NewName}");
    }
}