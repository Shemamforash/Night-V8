using DG.Tweening;
using UnityEngine;

public class ResourcesUiController : MonoBehaviour
{
    private static CanvasGroup _canvas;
    private static bool _hidden;

    public void Awake()
    {
        _canvas = GetComponent<CanvasGroup>();
    }

    public static void Show()
    {
        _hidden = false;
        _canvas.DOFade(1f, 1f).SetUpdate(UpdateType.Normal, true);
    }

    public static void Hide()
    {
        _hidden = true;
        _canvas.DOFade(0f, 1f).SetUpdate(UpdateType.Normal, true);
    }

    public static bool Hidden()
    {
        return _hidden;
    }
}