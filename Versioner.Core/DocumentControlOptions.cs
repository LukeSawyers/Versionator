namespace Versioner.Core;

/// <summary>
/// 
/// </summary>
/// <param name="LockCommitted">Locks committed files and prevents them from being changed</param>
public record DocumentControlOptions(
    string VersionNamingFormat = "{Name}-V{Version}",
    bool LockCommitted = true
);