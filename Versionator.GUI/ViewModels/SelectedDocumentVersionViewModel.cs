using System.Collections.ObjectModel;
using Versionator.GUI.ViewModels.BaseViewModels;

namespace Versionator.GUI.ViewModels;

public class SelectedDocumentVersionViewModel : BaseDocumentVersionViewModel
{
    public ObservableCollection<ChangeLogViewModel> ChangeLogs { get; init; }
}