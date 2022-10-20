using System;

namespace Versioner.Core;

/// <param name="CreationDate">The date that this version was created</param>
/// <param name="LastCheckInDate">The date that this version was last edited</param>
/// <param name="CommittedDate">The date that this version was committed. Null if the document is not committed</param>
/// <param name="Changes">The changes entered for this version</param>
public record DocumentVersionInfo(
    DateTime CreationDate,
    string Creator,
    DateTime LastCheckInDate,
    string LastEditor,
    DateTime? CommittedDate,
    string? Committer,
    ChangeLog[] Changes
)
{
    public DocumentVersionInfo() : this(
        DateTime.Now, "Anon", DateTime.Now, "Anon", null, null, Array.Empty<ChangeLog>()
    )
    {
    }
}