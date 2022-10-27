using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Versionator.GUI.Views;

public partial class DocumentVersionListItemView : UserControl
{
    public DocumentVersionListItemView()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}