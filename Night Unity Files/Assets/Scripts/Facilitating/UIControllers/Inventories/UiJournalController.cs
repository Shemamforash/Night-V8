﻿using System.Collections.Generic;
using DefaultNamespace;
using Facilitating.UIControllers.Inventories;
using Game.Global;
using SamsHelper.Libraries;
using SamsHelper.ReactiveUI.Elements;
using UnityEngine;

public class UiJournalController : UiInventoryMenuController
{
    private ListController _journalList;
    private static EnhancedText _journalDescription;

    protected override void Initialise()
    {
        _journalList.Initialise(typeof(JournalElement), o => { }, () => { });
        _journalList.SetOnItemHover(UpdateJournalDescription);
    }

    protected override void CacheElements()
    {
        _journalList = gameObject.FindChildWithName<ListController>("List");
        _journalDescription = gameObject.FindChildWithName<EnhancedText>("Text");
    }

    private void UpdateJournalDescription(object obj)
    {
        if (obj == null)
        {
            _journalDescription.SetText("No Journal Entries");
            return;
        }
        JournalEntry entry = (JournalEntry) obj;
        _journalDescription.SetText(entry.Contents);
    }

    protected override void OnShow()
    {
        _journalList.Show(GetAvailableJournalEntries);
    }

    private List<object> GetAvailableJournalEntries()
    {
        return JournalEntry.JournalEntries.ToObjectList();
    }

    private class JournalElement : ListElement
    {
        private EnhancedText _nameText;

        protected override void SetVisible(bool visible)
        {
            _nameText.gameObject.SetActive(visible);
        }

        protected override void CacheUiElements(Transform transform)
        {
            _nameText = transform.gameObject.FindChildWithName<EnhancedText>("Name");
        }

        protected override void Update(object o)
        {
            JournalEntry entry = (JournalEntry) o;
            _nameText.SetText(entry.Title);
        }

        protected override void UpdateCentreItemEmpty()
        {
            _nameText.SetText("-");
            _journalDescription.SetText("No Journal Entries Found");
        }

        public override void SetColour(Color c)
        {
            _nameText.SetColor(c);
        }
    }
}