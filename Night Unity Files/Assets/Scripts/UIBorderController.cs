using DG.Tweening;
using SamsHelper.Libraries;
using SamsHelper.ReactiveUI.Elements;
using UnityEngine;
using UnityEngine.UI;

public class UIBorderController : MonoBehaviour
{
    private Image _activeTop;
    private CanvasGroup _inactiveTop, _highlightCanvas;
    private const float FadeTime = 0.25f;
    [SerializeField] private BorderState _currentState;
    private bool _needsUpdate;
    private EnhancedButton _button;

    private enum BorderState
    {
        Inactive,
        Active,
        Selected
    }

    private void Awake()
    {
        GameObject top = gameObject.FindChildWithName("Top");
        _activeTop = top.FindChildWithName<Image>("Active");
        _inactiveTop = top.FindChildWithName<CanvasGroup>("Inactive");
        _highlightCanvas = gameObject.FindChildWithName<CanvasGroup>("Highlight");
        SetActive();
    }

    private void SetState(BorderState state)
    {
        _currentState = state;
        if (!gameObject.activeInHierarchy) _needsUpdate = true;
        else UpdateBorder();
    }

    private void OnEnable()
    {
        if (!_needsUpdate) return;
        UpdateBorder();
        _needsUpdate = false;
    }

    private void OnDisable()
    {
        _currentState = BorderState.Active;
        UpdateBorder(true);
    }

    private void UpdateBorder(bool instant = false)
    {
        bool currentTimeScale = DOTween.defaultTimeScaleIndependent;
        DOTween.defaultTimeScaleIndependent = true;
        switch (_currentState)
        {
            case BorderState.Inactive:
                MakeInactive(instant);
                break;
            case BorderState.Active:
                MakeActive(instant);
                break;
            case BorderState.Selected:
                MakeSelected(instant);
                break;
        }

        DOTween.defaultTimeScaleIndependent = currentTimeScale;
    }

    private void MakeInactive(bool instant = false)
    {
        float time = instant ? 0 : FadeTime;
        _activeTop.DOColor(UiAppearanceController.InvisibleColour, time);
        _inactiveTop.DOFade(0, time);
        _highlightCanvas.DOFade(0, time);
    }

    private void MakeActive(bool instant = false)
    {
        float time = instant ? 0 : FadeTime;
        _activeTop.DOColor(UiAppearanceController.InvisibleColour, time);
        _inactiveTop.DOFade(0.4f, time);
        _highlightCanvas.DOFade(0.15f, time);
    }

    private void MakeSelected(bool instant = false)
    {
        float time = instant ? 0 : FadeTime;
        if (_button.IsEnabled())
        {
            _activeTop.DOColor(Color.white, time);
            _inactiveTop.DOFade(1, time);
            _highlightCanvas.DOFade(0.75f, time);
            return;
        }

        _activeTop.DOColor(UiAppearanceController.FadedColour, time);
        _inactiveTop.DOFade(0.4f, time);
        _highlightCanvas.DOFade(0f, time);
    }

    public void SetInactive()
    {
        SetState(BorderState.Inactive);
    }

    public void SetActive()
    {
        SetState(BorderState.Active);
    }

    public void SetSelected()
    {
        SetState(BorderState.Selected);
    }

    public void SetButton(EnhancedButton enhancedButton)
    {
        _button = enhancedButton;
        transform.SetParent(_button.transform, false);
    }
}