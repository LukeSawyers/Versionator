
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

        await (version == null
            ? controller.CheckInCurrentVersionAsync(changes)
            : controller.CheckInVersionAsync(version.Value, changes));
    }
}