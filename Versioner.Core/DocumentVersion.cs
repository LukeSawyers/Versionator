using System;
using System.Linq;

namespace Versioner.Core;

public record struct DocumentVersion(int Major = 1, int Minor = 0, int Revision = 0)
{
    public static DocumentVersion Parse(string str)
    {
        var numbers = str.Split('.', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
            .Select(s => int.TryParse(s, out var i) ? i : (int?)null)
            .ToArray();

        return new DocumentVersion(
            numbers.ElementAtOrDefault(0) ?? 1,
            numbers.ElementAtOrDefault(1) ?? 0,
            numbers.ElementAtOrDefault(2) ?? 0
        );
    }
}