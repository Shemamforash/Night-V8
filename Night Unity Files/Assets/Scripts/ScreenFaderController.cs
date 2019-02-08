using DG.Tweening;
using Game.Combat.Generation;
using SamsHelper.Libraries;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ScreenFaderController : MonoBehaviour
{
    private static Image _faderImage;
    private static CanvasGroup _faderCanvas, _textCanvas;
    private static TextMeshProUGUI _text;
    private static Sequence _sequence;

    private void Awake()
    {
        _faderCanvas = GetComponent<CanvasGroup>();
        _faderImage = GetComponent<Image>();
        _textCanvas = gameObject.FindChildWithName<CanvasGroup>("Text Canvas");
        _text = gameObject.FindChildWithName<TextMeshProUGUI>("Text");
        bool isCombat = SceneManager.GetActiveScene().name == "Combat";
        float duration = isCombat ? 3f : 0.5f;
        float pause = isCombat ? 1f : 0.5f;
        _sequence = DOTween.Sequence();
        _sequence.AppendInterval(pause);
        _sequence.Append(_faderCanvas.DOFade(0, duration).SetUpdate(UpdateType.Normal, true));
    }

    private void Start()
    {
        if (CombatManager.Instance() == null) return;
        string text = CombatManager.GetCurrentRegion().Name;
        if (text == "") return;
        _text.text = text;
        _textCanvas.alpha = 1;
    }

    public static Sequence FadeIn(float duration)
    {
        ResetFader();
        _sequence = DOTween.Sequence();
        _sequence.Append(_faderCanvas.DOFade(1, duration).SetUpdate(UpdateType.Normal, true));
        return _sequence;
    }

    private static void ResetFader()
    {
        _textCanvas.alpha = 0;
        _text.text = "";
        _sequence?.Kill();
        Color resetColor = Color.black;
        resetColor.a = _faderImage.color.a;
        _faderImage.color = resetColor;
    }

    public static void FlashWhite(float duration, Color to)
    {
        ResetFader();
        _faderImage.color = Color.white;
        _faderCanvas.alpha = 1;
        _faderImage.DOColor(to, duration).SetUpdate(UpdateType.Normal, true);
    }
}