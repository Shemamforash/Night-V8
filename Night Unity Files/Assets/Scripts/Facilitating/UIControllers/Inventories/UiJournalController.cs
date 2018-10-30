﻿using System.Collections.Generic;
using DefaultNamespace;
using Facilitating.UIControllers;
using Facilitating.UIControllers.Inventories;
using Game.Global;
using SamsHelper.Libraries;
using SamsHelper.ReactiveUI.Elements;
using UnityEngine;

public class UiJournalController : UiInventoryMenuController
{
    private ListController _journalList;
    private static EnhancedText _journalDescription, _journalTitle;
    private CanvasGroup _noJournalGroup;

    protected override void Initialise()
    {
        _journalList.Initialise(typeof(JournalElement), o => { }, UiGearMenuController.Close);
        _journalList.SetOnItemHover(UpdateJournalDescription);
    }

    protected override void CacheElements()
    {
        _journalList = gameObject.FindChildWithName<ListController>("List");
        _journalDescription = gameObject.FindChildWithName<EnhancedText>("Text");
        _journalTitle = gameObject.FindChildWithName<EnhancedText>("Title");
        _noJournalGroup = gameObject.FindChildWithName<CanvasGroup>("No Journals");
    }

    private void UpdateJournalDescription(object obj)
    {
        if (obj == null)
        {
            _journalTitle.SetText("");
            _journalDescription.SetText("");
            _noJournalGroup.alpha = 1;
            return;
        }

        JournalEntry entry = (JournalEntry) obj;
        _noJournalGroup.alpha = 0;
        _journalTitle.SetText(entry.Title);
        _journalDescription.SetText(entry.Contents);
    }

    protected override void OnShow()
    {
        _journalList.Show(GetAvailableJournalEntries);
    }

    private List<object> GetAvailableJournalEntries()
    {
        for (int i = 0; i < 5; ++i) JournalEntry.GetEntry()?.Unlock();
        return JournalEntry.GetUnlockedEntries().ToObjectList();
    }

    private class JournalElement : ListElement
    {
        private EnhancedText _nameText;

        protected override void SetVisible(bool visible)
        {
            _nameText.SetColor(visible ? Color.white : UiAppearanceController.InvisibleColour);
        }

        protected override void CacheUiElements(Transform transform)
        {
            _nameText = transform.GetComponent<EnhancedText>();
        }

        protected override void Update(object o)
        {
            JournalEntry entry = (JournalEntry) o;
            _nameText.SetText(entry.Title);
        }

        protected override void UpdateCentreItemEmpty()
        {
            _nameText.SetText("");
            _journalDescription.SetText("No Journal Entries Found");
            _journalTitle.SetText("");
        }

        public override void SetColour(Color c)
        {
            _nameText.SetColor(c);
        }
    }
}