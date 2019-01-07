﻿using Facilitating.Persistence;
using Facilitating.UIControllers;
using SamsHelper.Libraries;
using SamsHelper.ReactiveUI.MenuSystem;

public class LoadSaveMenu : Menu
{
    private bool _loading;
    private SaveUiController _autoSaveUi, _manualSaveUi;
    private Save _autoSave, _manualSave;

    public override void Awake()
    {
        base.Awake();
        _autoSaveUi = gameObject.FindChildWithName<SaveUiController>("Auto Slot");
        _manualSaveUi = gameObject.FindChildWithName<SaveUiController>("Manual Slot");
    }

    public override void PreEnter()
    {
        base.Enter();
        _autoSave = SaveController.LoadAutoSave();
        _manualSave = SaveController.LoadManualSave();
        _autoSaveUi.SetSave(_autoSave);
        _manualSaveUi.SetSave(_manualSave);
    }

    public void LoadAutoSave()
    {
        if (_loading) return;
        _autoSaveUi.Load();
        _loading = true;
    }

    public void LoadManualSave()
    {
        if (_loading) return;
        _manualSaveUi.Load();
        _loading = true;
    }
}