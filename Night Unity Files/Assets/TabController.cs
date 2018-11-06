using DG.Tweening;
using SamsHelper.Libraries;
using UnityEngine;
using UnityEngine.UI;

public class TabController : MonoBehaviour
{
    private CanvasGroup _tabCanvas;
    private Image _glowImage;

    private void Awake()
    {
        _tabCanvas = GetComponent<CanvasGroup>();
        _glowImage = gameObject.FindChildWithName<Image>("Glow");
        _glowImage.SetAlpha(0);
    }

    public void Flash()
    {
        _glowImage.SetAlpha(1f);
        _glowImage.DOFade(0f, 1.5f);
    }

    public void FlashAndFade()
    {
        Flash();
        if (_tabCanvas.alpha == 0f) return;
        _tabCanvas.DOFade(0f, 0.5f);
    }

    public void FadeIn()
    {
        if (_tabCanvas.alpha == 1f) return;
        _tabCanvas.DOFade(1f, 1.5f);
    }
}