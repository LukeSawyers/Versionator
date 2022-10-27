using System;

namespace Versionator.Core;

public record ChangeLog(DateTime Timestamp, string Author, ChangeType Type, string Description)
{
    public ChangeLog(ChangeType type, string description) : this(DateTime.Now, Environment.UserName, type, description)
    {
    }
    
    public ChangeLog() : this(DateTime.Now, Environment.UserName, ChangeType.Unknown, "")
    {
    }
}