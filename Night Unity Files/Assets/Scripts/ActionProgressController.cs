using DG.Tweening;
using Game.Characters.CharacterActions;
using Game.Global;
using SamsHelper.Libraries;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ActionProgressController : MonoBehaviour
{
    private Image _left, _right;
    private CanvasGroup _barCanvas;
    private TextMeshProUGUI _text;
    private BaseCharacterAction _lastState;
    private Tweener _rightTween, _leftTween;

    private void Awake()
    {
        GameObject actionProgressObject = gameObject.FindChildWithName("Action Progress");
        _barCanvas = gameObject.FindChildWithName<CanvasGroup>("Bar");
        _left = actionProgressObject.FindChildWithName<Image>("Left");
        _right = actionProgressObject.FindChildWithName<Image>("Right");
        _text = gameObject.FindChildWithName<TextMeshProUGUI>("Text");
    }

    public void UpdateCurrentAction(BaseCharacterAction state)
    {
        if (state == null) return;
        if (_lastState == state && !state.ForceViewUpdate) return;
        state.ForceViewUpdate = false;
        _barCanvas.DOFade(state is Rest ? 0f : 1f, 0.5f);
        float initialDuration = state.GetInitialDuration();
        float remainingDuration = state.GetRemainingDuration();
        float timeToComplete = remainingDuration * WorldState.MinuteInSeconds;
        float startFill = remainingDuration / initialDuration;
        _left.fillAmount = startFill;
        _right.fillAmount = startFill;
        _leftTween?.Kill();
        _rightTween?.Kill();
        _leftTween = _left.DOFillAmount(0f, timeToComplete).SetUpdate(UpdateType.Manual).SetEase(Ease.Linear);
        _rightTween = _right.DOFillAmount(0f, timeToComplete).SetUpdate(UpdateType.Manual).SetEase(Ease.Linear);
        _text.text = state.GetDisplayName();
        _lastState = state;
    }

    private void Update()
    {
        if (WorldState.Paused()) return;
        DOTween.ManualUpdate(Time.deltaTime, Time.unscaledTime);
    }
}