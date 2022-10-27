using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Versionator.GUI.Views;

public partial class SelectedFileView : UserControl
{
    public SelectedFileView()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}