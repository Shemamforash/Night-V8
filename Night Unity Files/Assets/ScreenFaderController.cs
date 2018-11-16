using DG.Tweening;
using SamsHelper.Libraries;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ScreenFaderController : MonoBehaviour
{
    private static Image _faderImage;
    private static CanvasGroup _faderCanvas, _textCanvas;
    private static TextMeshProUGUI _text;

    private void Awake()
    {
        _faderCanvas = GetComponent<CanvasGroup>();
        _faderImage = GetComponent<Image>();
        _textCanvas = gameObject.FindChildWithName<CanvasGroup>("Text Canvas");
        _text = gameObject.FindChildWithName<TextMeshProUGUI>("Text");
    }

    public static void ShowText(string text)
    {
        if (_textCanvas == null) Initialise();
        _text.text = text;
        _textCanvas.alpha = 1;
    }

    public static void HideText()
    {
        if (_textCanvas == null) Initialise();
        _textCanvas.alpha = 0;
    }

    private static void Initialise()
    {
        GameObject.Find("Screen Fader").GetComponent<ScreenFaderController>().Awake();
    }

    public static void FadeIn(float duration)
    {
        if (_faderCanvas == null) Initialise();
        _faderCanvas.DOFade(1, duration).SetUpdate(UpdateType.Normal, true);
    }

    public static void FadeOut(float duration)
    {
        if (_faderCanvas == null) Initialise();
        Sequence sequence = DOTween.Sequence().SetUpdate(UpdateType.Normal, true);
        sequence.AppendInterval(1f);
        sequence.Append(_faderCanvas.DOFade(0, duration));
    }

    public static void SetAlpha(float alpha)
    {
        if (_faderCanvas == null) Initialise();
        _faderCanvas.alpha = alpha;
    }

    public static void FlashWhite(float duration)
    {
        if (_faderImage == null) Initialise();
        _faderImage.color = Color.white;
        _faderCanvas.alpha = 1;
        _faderImage.DOColor(Color.black, duration).SetUpdate(UpdateType.Normal, true);
    }
}