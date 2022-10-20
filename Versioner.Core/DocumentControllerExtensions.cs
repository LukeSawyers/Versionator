using System.Threading.Tasks;

namespace Versioner.Core;

public static class DocumentControllerExtensions
{
    public static async Task<CheckInResult?> CheckInCurrentVersionAsync(this DocumentController controller, ChangeLog[] changes)
    {
        var currentVersion = await controller.Accessor.GetCurrentVersionAsync();
        if (currentVersion is not DocumentVersion version)
        {
            return null;
        }

        return await controller.CheckInVersionAsync(version, changes);
    }
}