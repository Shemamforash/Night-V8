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
    private static Tweener _tweener;

    private void Awake()
    {
        _faderCanvas = GetComponent<CanvasGroup>();
        _faderImage = GetComponent<Image>();
        _textCanvas = gameObject.FindChildWithName<CanvasGroup>("Text Canvas");
        _text = gameObject.FindChildWithName<TextMeshProUGUI>("Text");
    }

    public static void ShowText(string text)
    {
        if (text == "") return;
        if (_textCanvas == null) Initialise();
        _text.text = text;
        _textCanvas.alpha = 1;
    }

    private static void Initialise()
    {
        GameObject.Find("Screen Fader").GetComponent<ScreenFaderController>().Awake();
    }

    private static void ClearText()
    {
        _text.text = "";
        _textCanvas.alpha = 0;
    }

    public static Tweener FadeIn(float duration)
    {
        ResetFader();
        _tweener = _faderCanvas.DOFade(1, duration).SetUpdate(UpdateType.Normal, true);
        return _tweener;
    }

    private static void ResetFader()
    {
        if (_faderCanvas == null) Initialise();
        ClearText();
        _tweener?.Kill();
        Color resetColor = Color.black;
        resetColor.a = _faderImage.color.a;
        _faderImage.color = resetColor;
    }

    public static void FadeOut(float duration)
    {
        ResetFader();
        _tweener = _faderCanvas.DOFade(0, duration).SetUpdate(UpdateType.Normal, true);
    }

    public static void SetAlpha(float alpha)
    {
        if (_faderCanvas == null) Initialise();
        _faderCanvas.alpha = alpha;
    }

    public static void FlashWhite(float duration, Color to)
    {
        if (_faderImage == null) Initialise();
        ClearText();
        _faderImage.color = Color.white;
        _faderCanvas.alpha = 1;
        _faderImage.DOColor(to, duration).SetUpdate(UpdateType.Normal, true);
    }
}