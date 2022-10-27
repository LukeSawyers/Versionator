using Avalonia.Media;
using Versionator.Core;
using Versionator.GUI.Models;

namespace Versionator.GUI.ViewModels;

public class ChangeLogViewModel : ViewModelBase
{
    public IBrush? TypeColor => Log?.Type.GetBrush();

    public ChangeLog? Log { get; init; }
}