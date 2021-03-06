﻿using Facilitating.Persistence;
using Facilitating.UIControllers;
using SamsHelper.Libraries;
using SamsHelper.ReactiveUI.MenuSystem;

public class LoadSaveMenu : Menu
{
    private bool _loading;
    private SaveUiController _autoSaveUi, _manualSaveUi;
    private Save _autoSave, _manualSave;
    private CloseButtonController _closeButton;

    protected override void Awake()
    {
        base.Awake();
        _autoSaveUi = gameObject.FindChildWithName<SaveUiController>("Auto Slot");
        _manualSaveUi = gameObject.FindChildWithName<SaveUiController>("Manual Slot");
        _closeButton = gameObject.FindChildWithName<CloseButtonController>("Back Button");
    }

    public override void PreEnter()
    {
        base.Enter();
        _autoSave = SaveController.LoadAutoSave();
        _manualSave = SaveController.LoadManualSave();
        SetMostRecentSave();
        _autoSaveUi.SetSave(_autoSave);
        _manualSaveUi.SetSave(_manualSave);
        _closeButton.Enable();
    }

    public override void Exit()
    {
        base.Exit();
        _closeButton.Disable();
    }

    private void SetMostRecentSave()
    {
        if (!_autoSave.Valid()) return;
        if (_autoSave.MoreRecentThan(_manualSave))
            _autoSave.SetMostRecent();
        else
            _manualSave.SetMostRecent();
    }

    public static void LoadMostRecentSave()
    {
        Save autoSave = SaveController.LoadAutoSave();
        Save manualSave = SaveController.LoadManualSave();
        if (autoSave.MoreRecentThan(manualSave) && autoSave.Valid()) autoSave.LoadFromSave();
        else manualSave.LoadFromSave();
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