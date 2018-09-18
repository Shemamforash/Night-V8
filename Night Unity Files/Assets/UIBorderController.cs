using DG.Tweening;
using SamsHelper.Libraries;
using SamsHelper.ReactiveUI.Elements;
using UnityEngine;
using UnityEngine.UI;

public class UIBorderController : MonoBehaviour
{
    private Image _activeTop, _inactiveTop;
    private const float FadeTime = 0.25f;
    [SerializeField] private BorderState _currentState;
    private bool _needsUpdate;

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
        _inactiveTop = top.FindChildWithName<Image>("Inactive");
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
        UpdateBorder(true);
    }
    
    private void UpdateBorder(bool instant = false)
    {
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

        DOTween.defaultTimeScaleIndependent = false;
    }

    private void MakeInactive(bool instant = false)
    {
        float time = instant ? 0 : FadeTime;
        _activeTop.DOColor(UiAppearanceController.InvisibleColour, time);
        _inactiveTop.DOColor(UiAppearanceController.InvisibleColour, time);
    }

    private void MakeActive(bool instant = false)
    {
        float time = instant ? 0 : FadeTime;
        _activeTop.DOColor(UiAppearanceController.InvisibleColour, time);
        _inactiveTop.DOColor(UiAppearanceController.FadedColour, time);
    }

    private void MakeSelected(bool instant = false)
    {
        float time = instant ? 0 : FadeTime;
        _activeTop.DOColor(Color.white, time);
        _inactiveTop.DOColor(Color.white, time);
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
}