using DG.Tweening;
using Game.Global;
using UnityEngine;

public class JournalSource : ContainerController
{
    private readonly JournalEntry _journalEntry;
    private CanvasGroup _journalIndicator;
    
    public JournalSource(Vector2 position, JournalEntry journalEntry) : base(position, "Journal")
    {
        _inventory.Name = "Journal";
        PrefabLocation = "Puddle";
        ImageLocation = "Journal";
        _journalEntry = journalEntry;
    }

    public override void Take()
    {
        _journalEntry.Unlock();
        _journalIndicator = GameObject.Find("Journal Indicator").GetComponent<CanvasGroup>();
        Sequence sequence = DOTween.Sequence();
        sequence.Append(_journalIndicator.DOFade(1f, 1f));
        sequence.AppendInterval(5f);
        sequence.Append(_journalIndicator.DOFade(0f, 1f));
        Fade();
    }
}