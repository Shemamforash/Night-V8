using DG.Tweening;
using SamsHelper.Libraries;
using UnityEngine;
using UnityEngine.UI;

public class TabController : MonoBehaviour
{
    private CanvasGroup _tabCanvas;
    private Image _glowImage;
    private Tweener _tabTween;
    private Tweener _glowTween;

    private void Awake()
    {
        _tabCanvas = GetComponent<CanvasGroup>();
        _glowImage = gameObject.FindChildWithName<Image>("Glow");
        _glowImage.SetAlpha(0);
    }

    public void InstantFade()
    {
        _tabTween?.Kill();
        _tabCanvas.alpha = 0f;
    }

    public void Flash()
    {
        _glowTween?.Kill();
        _glowImage.SetAlpha(1f);
        _glowTween = _glowImage.DOFade(0f, 1.5f).SetUpdate(UpdateType.Normal, true);
    }

    public void FlashAndFade()
    {
        Flash();
        if (_tabCanvas.alpha == 0f) return;
        _tabTween?.Kill();
        _tabTween = _tabCanvas.DOFade(0f, 0.5f).SetUpdate(UpdateType.Normal, true);
    }

    public void FadeIn()
    {
        if (_tabCanvas.alpha == 1f) return;
        _tabTween?.Kill();
        _tabTween = _tabCanvas.DOFade(1f, 1.5f).SetUpdate(UpdateType.Normal, true);
    }
}