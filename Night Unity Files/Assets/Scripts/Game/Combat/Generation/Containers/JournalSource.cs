using System;
using Game.Combat.Generation;
using Game.Global;
using UnityEngine;

public class JournalSource : ContainerController
{
    private JournalEntry _journalEntry;
    private CanvasGroup _journalIndicator;
    private bool _read;

    public JournalSource(Vector2 position) : base(position)
    {
        ImageLocation = "Journal";
    }

    public void SetEntry(JournalEntry journalEntry)
    {
        _journalEntry = journalEntry;
    }

    public override String GetContents()
    {
        return _journalEntry == null ? "Placeholder book" : _journalEntry.Title;
    }

    public override void Take()
    {
        base.Take();
        _read = true;
        _journalEntry.Unlock();
        CombatJournalController.ShowJournal(_journalEntry);
        CombatManager.GetCurrentRegion().ReadJournal = true;
    }

    protected override string GetLogText() => null;

    public bool Read() => _read;
}