using System;
using Avalonia.Media;
using Versionator.Core;

namespace Versionator.GUI.Models;

public static class ChangeTypeExtensions
{
    public static ISolidColorBrush GetBrush(this ChangeType type)
    {
        return type switch
        {
            ChangeType.Unknown => Brushes.Gray,
            ChangeType.Addition => Brushes.ForestGreen,
            ChangeType.Modification => Brushes.CornflowerBlue,
            ChangeType.Fixup => Brushes.Purple,
            ChangeType.Deletion => Brushes.IndianRed,
            _ => throw new ArgumentOutOfRangeException()
        };
    }
}