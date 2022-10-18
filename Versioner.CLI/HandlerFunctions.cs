
using Newtonsoft.Json;
using Versioner.Core;

public static class HandlerFunctions
{
    public static async Task CheckOutAsync(CheckOut args)
    {
        var controller = await DocumentController.CreateAsync(args.File);
        if (controller == null)
        {
            return;
        }

        await controller.CheckOutVersionAsync(DocumentVersion.Parse(args.Version));
    }
    
    public static async Task CheckInAsync(CheckIn args)
    {
        var controller = await DocumentController.CreateAsync(args.File);
        if (controller == null)
        {
            return;
        }

        IEnumerable<ChangeLog> GetChanges(string str, ChangeType type)
        {
            return str
                .Split(';', StringSplitOptions.RemoveEmptyEntries)
                .Select(c => new ChangeLog(type, c));
        }

        var changes = Enumerable.Empty<ChangeLog>()
            .Concat(GetChanges(args.UnknownChanges, ChangeType.Unknown))
            .Concat(GetChanges(args.AdditionChanges, ChangeType.Addition))
            .Concat(GetChanges(args.ModificationChanges, ChangeType.Modification))
            .Concat(GetChanges(args.DeletionChanges, ChangeType.Deletion))
            .ToArray();

        var version = args.Version == null
            ? (DocumentVersion?)null
            : DocumentVersion.Parse(args.Version);

        if (version == null)
        {
            await controller.CheckInCurrentVersionAsync(changes);
            Console.WriteLine($"Checked in the current version of {Path.GetFileName(args.File)}");
        }
        else
        {
            await controller.CheckInVersionAsync(version.Value, changes);
            Console.WriteLine($"Checked in {Path.GetFileName(args.File)} version {version.Value}");
        }
    }
    
    public static async Task ListVersionsAsync(ListVersions args)
    {
        var controller = await DocumentController.CreateAsync(args.File);
        if (controller == null)
        {
            return;
        }

        var index = await controller.GetVersionIndexAsync();
        var currentVersion = await controller.GetCheckedOutVersionAsync();

        foreach (var (version, info) in index)
        {
            Console.Write(version.ToVersionString());
            if (version == currentVersion)
            {
                Console.Write("* ");
            }
            Console.WriteLine();
        }
    }
}