using System;
using System.Collections.Generic;
using System.Reactive;
using System.Threading.Tasks;
using Avalonia.Threading;
using ReactiveUI;
using Versionator.Core;
using Versionator.GUI.Models;

namespace Versionator.GUI.ViewModels.BaseViewModels;

public abstract class BaseDocumentVersionViewModel : ViewModelBase, IDisposable
{
    public string? Version { get; private set; }
    
    public bool IsCheckedOut { get; private set; }
    
    public DocumentVersionInfo? VersionInfo { get; private set; }

    public DocumentVersion? DocumentVersion { get; init; }

    public DocumentControllerModel? Model
    {
        get => _model;
        init
        {
            this.RaiseAndSetIfChanged(ref _model, value);
            _documentControllerSubscription?.Dispose();
            _documentControllerSubscription = _model?.OnChanged.Subscribe(_ => UpdateAsync());
        }
    }

    public DocumentController? Controller => Model?.Controller;
    
    public ReactiveCommand<Unit, Unit> CheckOut { get; }
    public ReactiveCommand<Unit, Unit> Commit { get; }

    private IDisposable? _documentControllerSubscription;
    private DocumentControllerModel? _model;
    
    public BaseDocumentVersionViewModel()
    {
        CheckOut = ReactiveCommand.CreateFromTask(CheckOutImplAsync);
        Commit = ReactiveCommand.CreateFromTask(CommitImplAsync);
    }

    public async Task UpdateAsync()
    {
        if (Model == null || DocumentVersion == null)
        {
            return;
        }

        Version = DocumentVersion.ToVersionString();
        var accessor = Model.Controller.Accessor;
        VersionInfo = (await accessor.GetVersionIndexAsync()).GetValueOrDefault(DocumentVersion);
        IsCheckedOut = (await accessor.GetCurrentVersionAsync()).ToVersionString() == Version;
        
        Dispatcher.UIThread.Post(() =>
        {
            this.RaisePropertyChanged(nameof(Version));
            this.RaisePropertyChanged(nameof(VersionInfo));
            this.RaisePropertyChanged(nameof(IsCheckedOut));
        });
    }

    private async Task<Unit> CheckOutImplAsync()
    {
        if (Model == null ||Controller == null || Version == null)
        {
            return Unit.Default;
        }

        var options = await Controller.Accessor.GetOptionsAsync();
        await Controller.CheckOutVersionAsync(DocumentVersion.Parse(options.VersioningType, Version));
        
        Model.CallChanged();
        
        return Unit.Default;
    }

    private async Task<Unit> CommitImplAsync()
    {
        if (Model == null || Controller == null || Version == null)
        {
            return Unit.Default;
        }

        var options = await Controller.Accessor.GetOptionsAsync();
        await Controller.CommitVersionAsync(DocumentVersion.Parse(options.VersioningType, Version));

        Model.CallChanged();
        
        return Unit.Default;
    }

    public void Dispose()
    {
        _documentControllerSubscription?.Dispose();
        CheckOut.Dispose();
        Commit.Dispose();
    }
}