namespace Versioner.Core;

public enum VersioningType
{
    Semantic3,
    Semantic2,
    Semantic1,
    Custom
}

/// <summary>
/// 
/// </summary>
/// <param name="LockCommitted">Locks committed files and prevents them from being changed</param>
public record DocumentControlOptions(
    string VersionNamingFormat = "{Name}-V{Version}",
    VersioningType VersioningType = VersioningType.Semantic3,
    bool LockCommitted = true
);