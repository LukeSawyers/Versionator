using System;
using System.Linq;
using OneOf;

namespace Versioner.Core;

public class DocumentVersion : OneOfBase<string, Version>
{
    private readonly VersioningType _type;

    public DocumentVersion(VersioningType type, OneOf<string, Version> input) : base(input)
    {
        _type = type;
    }

    public static DocumentVersion Parse(VersioningType type, string str)
    {
        if (type == VersioningType.Custom)
        {
            return new DocumentVersion(type, str);
        }

        var numbers = str.Split('.', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
            .Select(s => int.TryParse(s, out var i) ? i : (int?)null)
            .ToArray();

        var version = type switch
        {
            VersioningType.Semantic3 => new Version(
                numbers.ElementAtOrDefault(0) ?? 0,
                numbers.ElementAtOrDefault(1) ?? 0,
                numbers.ElementAtOrDefault(2) ?? 1
            ),
            VersioningType.Semantic2 => new Version(
                numbers.ElementAtOrDefault(0) ?? 0,
                numbers.ElementAtOrDefault(1) ?? 1
            ),
            VersioningType.Semantic1 => new Version(
                numbers.ElementAtOrDefault(0) ?? 1, 0
            ),
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };

        return new DocumentVersion(type, version);
    }

    public string ToVersionString()
    {
        return Match(
            str => str,
            ver => string.Join(
                '.',
                _type switch
                {
                    VersioningType.Semantic4 => new[] { ver.Major, ver.Minor, ver.Build, ver.Revision },
                    VersioningType.Semantic3 => new[] { ver.Major, ver.Minor, ver.Build },
                    VersioningType.Semantic2 => new[] { ver.Major, ver.Minor },
                    VersioningType.Semantic1 => new[] { ver.Major },
                    _ => throw new ArgumentOutOfRangeException()
                }
            )
        );
    }
}