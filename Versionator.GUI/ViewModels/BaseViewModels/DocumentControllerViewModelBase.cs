using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Reactive;
using System.Threading.Tasks;
using Avalonia.Threading;
using DynamicData;
using ReactiveUI;
using Versionator.Core;
using Versionator.GUI.Models;

namespace Versionator.GUI.ViewModels.BaseViewModels;

public abstract class DocumentControllerViewModelBase : ViewModelBase, IDisposable
{
    public DocumentControllerModel? Model
    {
        get => _model;
        init
        {
            this.RaiseAndSetIfChanged(ref _model, value);
            _controllerChangedSubscription?.Dispose();
            _controllerChangedSubscription = _model?.OnChanged.Subscribe(_ => ControllerChanged());
            ControllerChanged();
        }
    }

    public DocumentController? Controller => Model?.Controller;

    public DocumentControlOptions? Options { get; private set; }
    public string? CurrentVersion { get; private set; }
    public ObservableCollection<DocumentVersionListItemViewModel> Versions { get; } = new();

    public ReactiveCommand<Unit, Unit> OpenFile { get; }
    public ReactiveCommand<Unit, Unit> OpenFileLocation { get; }

    private IDisposable? _controllerChangedSubscription;
    private DocumentControllerModel? _model;

    public DocumentControllerViewModelBase()
    {
        OpenFile = ReactiveCommand.CreateFromTask(OpenFileImplAsync);
        OpenFileLocation = ReactiveCommand.CreateFromTask(OpenFileLocationImplAsync);
    }

    private async Task<Unit> OpenFileImplAsync()
    {
        if (Controller == null)
        {
            return Unit.Default;
        }

        if (OperatingSystem.IsLinux())
        {
            Process.Start("xdg-open", $"\"{Controller.FilePath}\"");
        }

        return Unit.Default;
    }

    private async Task<Unit> OpenFileLocationImplAsync()
    {
        if (Controller == null)
        {
            return Unit.Default;
        }

        if (OperatingSystem.IsLinux())
        {
            Process.Start("nautilus", $"\"{Controller.FileDirectory}\"");
        }

        return Unit.Default;
    }

    public async Task ControllerChanged()
    {
        this.RaisePropertyChanged(nameof(Controller));
        this.RaisePropertyChanged(nameof(Controller.FilePath));
        this.RaisePropertyChanged(nameof(Controller.FileDirectory));
        this.RaisePropertyChanged(nameof(Controller.Accessor.FileName));

        if (Controller == null)
        {
            return;
        }

        Options = await Controller.Accessor.GetOptionsAsync();
        var versionIndex = await Controller.Accessor.GetVersionIndexAsync();
        var versions = versionIndex
            .Select(p =>
            {
                var model = new DocumentVersionListItemViewModel()
                {
                    Model = Model,
                    DocumentVersion = p.Key
                };
                return model;
            })
            .ToArray();

        await Task.WhenAll(versions.Select(v => v.UpdateAsync()));

        CurrentVersion = (await Controller.Accessor.GetCurrentVersionAsync()).ToVersionString();

        Dispatcher.UIThread.Post(() =>
        {
            Versions.Clear();
            Versions.AddRange(versions);
            this.RaisePropertyChanged(nameof(Options));
            this.RaisePropertyChanged(nameof(Versions));
            this.RaisePropertyChanged(nameof(CurrentVersion));
        });
    }

    public void Dispose()
    {
        _controllerChangedSubscription?.Dispose();
    }
}