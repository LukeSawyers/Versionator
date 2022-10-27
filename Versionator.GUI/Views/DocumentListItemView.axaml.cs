using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Versionator.GUI.Views;

public partial class DocumentListItemView : UserControl
{
    public DocumentListItemView()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}