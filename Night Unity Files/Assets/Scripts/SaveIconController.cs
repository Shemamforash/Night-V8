using DG.Tweening;
using Facilitating.Persistence;
using Game.Global;
using UnityEngine;

public class SaveIconController : MonoBehaviour
{
    private static CanvasGroup _saveCanvas;
    private static Sequence _sequence;

    public void Awake()
    {
        _saveCanvas = gameObject.GetComponent<CanvasGroup>();
    }

    public static void Save()
    {
        SaveController.SaveGame();
        _saveCanvas.alpha = 1;
        _sequence?.Kill();
        Sequence sequence = DOTween.Sequence();
        sequence.AppendInterval(1f);
        sequence.Append(_saveCanvas.DOFade(0f, 1f));
        sequence.SetUpdate(UpdateType.Normal, true);
        _sequence = sequence;
    }
}