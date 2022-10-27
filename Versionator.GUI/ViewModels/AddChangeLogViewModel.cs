using System;
using Avalonia.Media;
using ReactiveUI;
using Versionator.Core;
using Versionator.GUI.Models;

namespace Versionator.GUI.ViewModels;

public class AddChangeLogViewModel : ViewModelBase
{
    public ChangeType Type
    {
        get => _type;
        set
        {
            this.RaiseAndSetIfChanged(ref _type, value);
            TypeColor = value.GetBrush();
        }
    }

    public string Description { get; set; } = "";

    public IBrush TypeColor
    {
        get => _typeColour;
        private set
        {
            this.RaiseAndSetIfChanged(ref _typeColour, value);
        }
    }
    
    public ChangeType[] AllTypes { get; } = Enum.GetValues<ChangeType>();

    private ChangeType _type = ChangeType.Unknown;
    private IBrush _typeColour = Brushes.Cyan;
    
    public ChangeLog ToChangeLog() => new(Type, Description);
}