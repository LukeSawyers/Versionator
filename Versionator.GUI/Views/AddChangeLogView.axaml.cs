using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Versionator.GUI.Views;

public partial class AddChangeLogView : UserControl
{
    public AddChangeLogView()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}