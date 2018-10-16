using Game.Global;
using UnityEngine;

public class JournalSource : ContainerController
{
    private JournalEntry _journalEntry;
    private CanvasGroup _journalIndicator;

    public JournalSource(Vector2 position) : base(position)
    {
        ImageLocation = "Journal";
    }

    public void SetEntry(JournalEntry journalEntry)
    {
        _journalEntry = journalEntry;
    }

    public override void Take()
    {
        _journalEntry.Unlock();
        CombatJournalController.ShowJournal(_journalEntry);
    }
}