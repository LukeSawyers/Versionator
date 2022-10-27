using System;
using System.Reactive;
using System.Reactive.Subjects;
using Versionator.Core;

namespace Versionator.GUI.Models;

public class DocumentControllerModel
{
    public IObservable<Unit> OnChanged => _changedSubject;

    public DocumentController Controller { get; }

    private readonly Subject<Unit> _changedSubject = new();

    public DocumentControllerModel(DocumentController controller)
    {
        Controller = controller;
    }

    public void CallChanged()
    {
        _changedSubject.OnNext(Unit.Default);
    }
}