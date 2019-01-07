using DG.Tweening;
using Facilitating.Persistence;
using Game.Global;
using SamsHelper.Libraries;
using SamsHelper.ReactiveUI.Elements;
using UnityEngine;

public class SaveIconController : MonoBehaviour
{
    private static CanvasGroup _saveCanvas;
    private static Sequence _sequence;
    private static EnhancedText _saveText;

    public void Awake()
    {
        _saveCanvas = gameObject.GetComponent<CanvasGroup>();
        _saveText = gameObject.FindChildWithName<EnhancedText>("Text");
    }

    public static void Save()
    {
        SaveController.ManualSave();
        _saveText.SetText("Saved");
        Flash();
    }

    private static void Flash()
    {
        _saveCanvas.alpha = 1;
        _sequence?.Kill();
        Sequence sequence = DOTween.Sequence();
        sequence.AppendInterval(1f);
        sequence.Append(_saveCanvas.DOFade(0f, 1f));
        sequence.SetUpdate(UpdateType.Normal, true);
        _sequence = sequence;
    }

    public static void AutoSave()
    {
        SaveController.AutoSave();
        _saveText.SetText("Autosaved");
        Flash();
    }
}