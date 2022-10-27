using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Versionator.GUI.Views;

public partial class SelectedDocumentVersionView : UserControl
{
    public SelectedDocumentVersionView()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}