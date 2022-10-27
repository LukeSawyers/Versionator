using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reactive;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Threading;
using DynamicData;
using ReactiveUI;
using Versionator.Core;
using Versionator.GUI.Models;

namespace Versionator.GUI.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    public Window? Window { get; init; }
    
    public ObservableCollection<DocumentListItemViewModel> AllFiles { get; } = new();

    public DocumentListItemViewModel? SelectedListItem
    {
        get => _selectedListItem;
        set
        {
            this.RaiseAndSetIfChanged(ref _selectedListItem, value);
            this.RaisePropertyChanged(nameof(IsFileSelected));
            if (value != null)
            {
                SelectedFile = new SelectedFileViewModel()
                {
                    Model = value.Model,
                    Window = Window
                };
            }
        }
    }

    public SelectedFileViewModel? SelectedFile
    {
        get => _selectedFile;
        set
        {
            this.RaiseAndSetIfChanged(ref _selectedFile, value);
            _selectedFile?.ControllerChanged();
        }
    }

    public ReactiveCommand<Unit, Unit> Reload { get; }
    public ReactiveCommand<Unit,Unit> OpenFileAsync { get; }
    public ReactiveCommand<Unit, Unit> RemoveFileAsync { get; }

    public bool IsFileSelected => _selectedListItem != null;
    
    private DocumentListItemViewModel? _selectedListItem;
    private SelectedFileViewModel? _selectedFile;

    public MainWindowViewModel()
    {
        Reload = ReactiveCommand.CreateFromTask(ReloadImplAsync);
        OpenFileAsync = ReactiveCommand.CreateFromTask(OpenFileImplAsync);
        RemoveFileAsync = ReactiveCommand.CreateFromTask(RemoveFileImplAsync);
        Reload.Execute();
    }

    private async Task OpenFileImplAsync()
    {
        var dialog = new OpenFileDialog()
        {
            Title = "Open File to Version"
        };
        var files = await dialog.ShowAsync(Window!);
        if (files == null)
        {
            return;
        }
        
        var index = new OpenFileIndex();
        await index.AddOpenedFilesAsync(files);
        await ReloadImplAsync();
    }
    
    private async Task RemoveFileImplAsync()
    {
        if (_selectedListItem?.Controller?.FilePath is not {} filePath)
        {
            return;
        }
        
        var index = new OpenFileIndex();
        await index.RemoveOpenedFileAsync(filePath);
        await ReloadImplAsync();
    }

    private async Task ReloadImplAsync()
    {
        var index = new OpenFileIndex();
        var files = await index.GetOpenedFilesAsync();
        var controllers = new List<DocumentListItemViewModel>();
        foreach (var file in files)
        {
            var controller = await DocumentController.CreateAsync(file);
            if (controller != null)
            {
                controllers.Add(new DocumentListItemViewModel()
                {
                    Model =  new DocumentControllerModel(controller)
                });
            }
        }
        
        Dispatcher.UIThread.Post(() =>
        {
            foreach (var vm in AllFiles)
            {
                vm.Dispose();
            }
            
            AllFiles.Clear();
            AllFiles.AddRange(controllers);
            this.RaisePropertyChanged(nameof(AllFiles));
            foreach (var vm in AllFiles)
            {
                vm?.ControllerChanged();
            }
        });
    }
}