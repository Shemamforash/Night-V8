using System;
using Game.Characters;
using Game.Combat.Generation;
using Game.Global;
using UnityEngine;

public class JournalSource : ContainerController
{
    private JournalEntry _journalEntry;
    private CanvasGroup _journalIndicator;
    private bool _read;
    private static Sprite _journalSprite;

    public JournalSource(Vector2 position) : base(position)
    {
        if (_journalSprite == null) _journalSprite = Resources.Load<Sprite>("Images/Container Symbols/Journal");
        Sprite = _journalSprite;
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
        CharacterManager.CurrentRegion().JournalIsHere = false;
    }

    protected override string GetLogText() => null;

    public bool Read() => _read;
}