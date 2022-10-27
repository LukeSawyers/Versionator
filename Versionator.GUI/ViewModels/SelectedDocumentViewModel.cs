using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Threading;
using ReactiveUI;
using Versionator.Core;
using Versionator.GUI.Models;
using Versionator.GUI.ViewModels.BaseViewModels;
using Versionator.GUI.Views;

namespace Versionator.GUI.ViewModels;

public class SelectedFileViewModel : DocumentControllerViewModelBase
{
    public Window? Window { get; init; }

    public DocumentVersionListItemViewModel? SelectedVersionItem
    {
        get
        {
            if (SelectedVersion == null)
            {
                return null;
            }

            return new()
            {
                Model = SelectedVersion.Model,
                DocumentVersion = SelectedVersion.DocumentVersion
            };
        }
        set
        {
            SelectedVersion?.Dispose();

            if (value == null)
            {
                SelectedVersion = null;
            }
            else
            {
                var changeLogs = value.VersionInfo?.Changes.Select(c => new ChangeLogViewModel()
                {
                    Log = c
                }) ?? Array.Empty<ChangeLogViewModel>();

                SelectedVersion = new SelectedDocumentVersionViewModel()
                {
                    Model = value.Model,
                    DocumentVersion = value.DocumentVersion,
                    ChangeLogs = new(changeLogs)
                };

                SelectedVersion.UpdateAsync();
            }
        }
    }

    public SelectedDocumentVersionViewModel? SelectedVersion
    {
        get => _selectedVersion;
        set
        {
            _selectedVersion?.Dispose();
            _selectedVersion = value;
            this.RaisePropertyChanged();
            this.RaisePropertyChanged(nameof(SelectedVersionItem));
        }
    }

    public ReactiveCommand<Unit, Unit> StartCheckin { get; }
    public ReactiveCommand<Unit, Unit> CancelCheckin { get; }
    public ReactiveCommand<Unit, Unit> CompleteCheckin { get; }

    public bool IsCheckingIn
    {
        get => _isCheckingIn;
        private set => this.RaiseAndSetIfChanged(ref _isCheckingIn, value);
    }

    public string? CheckInVersion
    {
        get => _checkInVersion;
        set { this.RaiseAndSetIfChanged(ref _checkInVersion, value); }
    }

    public ObservableCollection<AddChangeLogViewModel> ChangeLogItems { get; } = new();
    public ReactiveCommand<Unit, Unit> AddChangeLog { get; }

    private bool _isCheckingIn = false;
    private string? _checkInVersion;
    private SelectedDocumentVersionViewModel? _selectedVersion;

    public SelectedFileViewModel()
    {
        StartCheckin = ReactiveCommand.CreateFromTask(StartCheckInImplAsync);
        CancelCheckin = ReactiveCommand.CreateFromTask(CancelCheckInImplAsync);
        CompleteCheckin = ReactiveCommand.CreateFromTask(CompleteCheckInImplAsync);
        AddChangeLog = ReactiveCommand.CreateFromTask(AddChangeLogImplAsync);
    }

    private async Task AddChangeLogImplAsync()
    {
        ChangeLogItems.Add(new AddChangeLogViewModel()
        {
            Type = ChangeType.Modification,
            Description = "{Describe your changes}"
        });
    }

    private async Task CancelCheckInImplAsync() => IsCheckingIn = false;

    private async Task StartCheckInImplAsync()
    {
        ChangeLogItems.Clear();
        await AddChangeLogImplAsync();

        CheckInVersion = SelectedVersion?.Version;
        IsCheckingIn = true;
    }

    private async Task CompleteCheckInImplAsync()
    {
        if (Model?.Controller == null)
        {
            return;
        }

        var changeLogs = ChangeLogItems
            .Select(i => i.ToChangeLog())
            .ToArray();

        if (Controller == null || CheckInVersion == null)
        {
            return;
        }

        var options = await Controller.Accessor.GetOptionsAsync();
        var checkInResult = await Controller.CheckInVersionAsync(
            DocumentVersion.Parse(options.VersioningType, CheckInVersion), changeLogs
        );

        Dispatcher.UIThread.Post(() =>
        {
            Model.CallChanged();
            IsCheckingIn = false;
        });
    }
}